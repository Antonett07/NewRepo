using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDay.Web.Services
{
    public class RaceDayApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public RaceDayApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AttachSessionHeaders()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            var userId = context.Session.GetString("UserId");
            var role = context.Session.GetString("UserRole");

            _httpClient.DefaultRequestHeaders.Remove("X-User-Id");
            _httpClient.DefaultRequestHeaders.Remove("X-User-Role");

            if (!string.IsNullOrEmpty(userId))
                _httpClient.DefaultRequestHeaders.Add("X-User-Id", userId);
            if (!string.IsNullOrEmpty(role))
                _httpClient.DefaultRequestHeaders.Add("X-User-Role", role);
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            AttachSessionHeaders();
            var response = await _httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode) return default;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        public async Task<(bool Success, string Content)> PostJsonAsync<T>(string endpoint, T payload)
        {
            AttachSessionHeaders();
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            var body = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, body);
        }

        // Sends image file from MVC to API -> API uploads to Azure Blob Storage
        public async Task<string?> UploadImageToApiAsync(string endpoint, IFormFile file)
        {
            AttachSessionHeaders();
            using var formData = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            formData.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync(endpoint, formData);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadAsStringAsync(); // Returns Blob URL
        }
    }
}