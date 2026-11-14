using Microsoft.AspNetCore.Http;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Services
{
    public interface IRaceDayApiClient
    {
        Task<AuthResponseViewModel?> LoginAsync(LoginViewModel model);
        Task<bool> RegisterAsync(RegisterViewModel model);
        Task<UserProfileViewModel?> GetProfileAsync(int? userId, string? token);
        Task<bool> UpdateProfileAsync(UserProfileViewModel model, string? token);

        Task<List<EventViewModel>> GetEventsAsync(string? search = null, string? location = null, string? eventType = null);
        Task<EventViewModel?> GetEventByIdAsync(int id);
        Task<List<EventViewModel>> GetOrganiserEventsAsync(int organiserId, string? token);
        Task<bool> CreateEventAsync(CreateEventViewModel model, int organiserId, string? token);
        Task<bool> UpdateEventAsync(int id, CreateEventViewModel model, int organiserId, string? token);
        Task<bool> DeleteEventAsync(int id, int organiserId, string? token);

        Task<List<CategoryViewModel>> GetCategoriesAsync(int eventId);
        Task<bool> AddCategoryAsync(int eventId, CreateCategoryViewModel model, string? token);
        Task<bool> DeleteCategoryAsync(int categoryId, string? token);

        Task<bool> EnrolAsync(int categoryId, int participantId, string? token);
        Task<List<EnrolmentViewModel>> GetMyEnrolmentsAsync(int participantId, string? token);
        Task<List<EnrolmentViewModel>> GetEventEnrolmentsAsync(int eventId, string? token);

        Task<bool> SubmitResultAsync(CreateResultViewModel model, string? token);
        Task<List<ResultViewModel>> GetMyResultsAsync(int participantId, string? token);

        Task<string?> UploadBannerImageAsync(IFormFile file, string? token);
        Task<string?> UploadProfilePictureAsync(IFormFile file, string? token);
    }
}
