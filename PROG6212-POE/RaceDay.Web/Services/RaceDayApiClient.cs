using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Services
{
    public class RaceDayApiClient : IRaceDayApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public RaceDayApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string url, int? userId = null, string? role = null, string? token = null)
        {
            var request = new HttpRequestMessage(method, url);
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            if (userId.HasValue)
            {
                request.Headers.Add("X-User-Id", userId.Value.ToString());
            }
            if (!string.IsNullOrEmpty(role))
            {
                request.Headers.Add("X-User-Role", role);
            }
            return request;
        }

        public async Task<AuthResponseViewModel?> LoginAsync(LoginViewModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", new
                {
                    Email = model.Email,
                    Password = model.Password
                });

                if (!response.IsSuccessStatusCode) return null;

                var data = await response.Content.ReadFromJsonAsync<AuthResponseViewModel>(_jsonOptions);
                return data;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", new
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = model.Password,
                    RoleID = model.RoleID,
                    PhoneNumber = model.PhoneNumber
                });

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserProfileViewModel?> GetProfileAsync(int? userId, string? token)
        {
            try
            {
                var url = userId.HasValue ? $"api/auth/profile?id={userId.Value}" : "api/auth/profile";
                var req = CreateRequest(HttpMethod.Get, url, userId, null, token);
                var response = await _httpClient.SendAsync(req);

                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<UserProfileViewModel>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateProfileAsync(UserProfileViewModel model, string? token)
        {
            try
            {
                var req = CreateRequest(HttpMethod.Put, "api/auth/profile", model.UserId, model.Role, token);
                req.Content = JsonContent.Create(new
                {
                    UserId = model.UserId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    ProfilePictureUrl = model.ProfilePictureUrl
                });

                var response = await _httpClient.SendAsync(req);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<EventViewModel>> GetEventsAsync(string? search = null, string? location = null, string? eventType = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (!string.IsNullOrWhiteSpace(location)) queryParams.Add($"location={Uri.EscapeDataString(location)}");
                if (!string.IsNullOrWhiteSpace(eventType)) queryParams.Add($"eventType={Uri.EscapeDataString(eventType)}");

                var url = "api/events" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode) return new List<EventViewModel>();

                var events = await response.Content.ReadFromJsonAsync<List<EventViewModel>>(_jsonOptions);
                return events ?? new List<EventViewModel>();
            }
            catch
            {
                return new List<EventViewModel>();
            }
        }

        public async Task<EventViewModel?> GetEventByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/events/{id}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<EventViewModel>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<EventViewModel>> GetOrganiserEventsAsync(int organiserId, string? token)
        {
            try
            {
                var req = CreateRequest(HttpMethod.Get, $"api/events/organiser/{organiserId}", organiserId, "Organiser", token);
                var response = await _httpClient.SendAsync(req);

                if (!response.IsSuccessStatusCode) return new List<EventViewModel>();

                return await response.Content.ReadFromJsonAsync<List<EventViewModel>>(_jsonOptions) ?? new List<EventViewModel>();
            }
            catch
            {
                return new List<EventViewModel>();
            }
        }

        public async Task<bool> CreateEventAsync(CreateEventViewModel model, int organiserId, string? token)
        {
            try
            {
                var req = CreateRequest(HttpMethod.Post, "api/events", organiserId, "Organiser", token);
                req.Content = JsonContent.Create(new
                {
                    OrganiserId = organiserId,
                    EventName = model.EventName,
                    Description = model.Description,
                    Location = model.Location,
                    EventDate = model.EventDate,
                    EventType = model.EventType,
                    Distance = model.Distance,
                    BannerImageUrl = model.ExistingBannerUrl
                });

                var response = await _httpClient.SendAsync(req);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateEventAsync(int id, CreateEventViewModel model, int organiserId, string? token)
        {
            try
            {
                var req = CreateRequest(HttpMethod.Put, $"api/events/{id}", organiserId, "Organiser", token);
                req.Content = JsonContent.Create(new
                {
                    OrganiserId = organiserId,
                    EventName = model.EventName,
                    Description = model.Description,
                    Location = model.Location,
                    EventDate = model.EventDate,
                    EventType = model.EventType,
                    Distance = model.Distance,
                    BannerImageUrl = model.ExistingBannerUrl
                });

                var response = await _httpClient.SendAsync(req);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteEventAsync(int id, int organiserId, string? token)
        {
            try
            {
                var req = CreateRequest(HttpMethod.Delete, $"api/events/{id}?organiserId={organiserId}", organiserId, "Organiser", token);
                var response = await _httpClient.SendAsync(req);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<CategoryViewModel>> GetCategoriesAsync(int eventId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/events/{eventId}/categories");
                if (!response.IsSuccessStatusCode) return new List<CategoryViewModel>();

                return await response.Content.ReadFromJsonAsync<List<CategoryViewModel>>(_jsonOptions) ?? new List<CategoryViewModel>();
            }
            catch
            {
                return new List<CategoryViewModel>();
            }
        }

        public async Task<bool> AddCategoryAsync(int eventId, CreateCategoryViewModel model, string? token)
        {
            try
            {
                var req = CreateRequest(HttpMethod.Post, $"api/events/{eventId}/categories", null, "Organiser", token);
                req.Content = JsonContent.Create(new
                {
                    CategoryName = model.CategoryName,
                    DistanceKM = model.DistanceKM,
                    EntryFee = model.EntryFee,
                    MaxParticipants = model.MaxParticipants
                });

                var response = await _httpClient.SendAsync(req);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId, string? token)
        {
            try
            {
                var req = CreateRequest(HttpMethod.Delete, $"api/categories/{categoryId}", null, "Organiser", token);
                var response = await _httpClient.SendAsync(req);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EnrolAsync(int categoryId, int participantId, string? token)
        {
            try
            {
                var req = CreateRequest(HttpMethod.Post, "api/enrolments", participantId, "Participant", token);
                req.Content = JsonContent.Create(new
                {
                    CategoryId = categoryId,
                    ParticipantId = participantId,
                    PaymentStatus = "Confirmed"
                });

                var response = await _httpClient.SendAsync(req);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<EnrolmentViewModel>> GetMyEnrolmentsAsync(int participantId, string? token)
        {
            try
            {
                var req = CreateRequest(HttpMethod.Get, $"api/enrolments/my-entries?participantId={participantId}", participantId, "Participant", token);
                var response = await _httpClient.SendAsync(req);

                if (!response.IsSuccessStatusCode) return new List<EnrolmentViewModel>();

                return await response.Content.ReadFromJsonAsync<List<EnrolmentViewModel>>(_jsonOptions) ?? new List<EnrolmentViewModel>();
            }
            catch
            {
                return new List<EnrolmentViewModel>();
            }
        }

        public async Task<List<EnrolmentViewModel>> GetEventEnrolmentsAsync(int eventId, string? token)
        {
            try
            {
                var req = CreateRequest(HttpMethod.Get, $"api/events/{eventId}/participants", null, "Organiser", token);
                var response = await _httpClient.SendAsync(req);

                if (!response.IsSuccessStatusCode) return new List<EnrolmentViewModel>();

                return await response.Content.ReadFromJsonAsync<List<EnrolmentViewModel>>(_jsonOptions) ?? new List<EnrolmentViewModel>();
            }
            catch
            {
                return new List<EnrolmentViewModel>();
            }
        }

        public async Task<bool> SubmitResultAsync(CreateResultViewModel model, string? token)
        {
            try
            {
                var req = CreateRequest(HttpMethod.Post, "api/results", null, "Organiser", token);
                req.Content = JsonContent.Create(new
                {
                    EnrolmentId = model.EnrolmentId,
                    FinishTimeString = model.FinishTimeString,
                    OverallPosition = model.OverallPosition,
                    CategoryPosition = model.CategoryPosition,
                    Status = model.Status
                });

                var response = await _httpClient.SendAsync(req);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<ResultViewModel>> GetMyResultsAsync(int participantId, string? token)
        {
            try
            {
                var req = CreateRequest(HttpMethod.Get, $"api/results/my-results?participantId={participantId}", participantId, "Participant", token);
                var response = await _httpClient.SendAsync(req);

                if (!response.IsSuccessStatusCode) return new List<ResultViewModel>();

                return await response.Content.ReadFromJsonAsync<List<ResultViewModel>>(_jsonOptions) ?? new List<ResultViewModel>();
            }
            catch
            {
                return new List<ResultViewModel>();
            }
        }

        public async Task<string?> UploadBannerImageAsync(IFormFile file, string? token)
        {
            return await UploadFileAsync(file, "event-banner", token);
        }

        public async Task<string?> UploadProfilePictureAsync(IFormFile file, string? token)
        {
            return await UploadFileAsync(file, "profile-picture", token);
        }

        private async Task<string?> UploadFileAsync(IFormFile file, string endpoint, string? token)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var stream = file.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
                content.Add(fileContent, "file", file.FileName);

                var req = CreateRequest(HttpMethod.Post, $"api/uploads/{endpoint}", null, null, token);
                req.Content = content;

                var response = await _httpClient.SendAsync(req);
                if (!response.IsSuccessStatusCode) return null;

                var result = await response.Content.ReadFromJsonAsync<UploadResultResponse>(_jsonOptions);
                return result?.Url;
            }
            catch
            {
                return null;
            }
        }

        private record UploadResultResponse(string Url);
    }
}
