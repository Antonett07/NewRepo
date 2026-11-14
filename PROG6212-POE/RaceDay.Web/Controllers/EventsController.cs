using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers
{
    public class EventsController : Controller
    {
        private readonly IRaceDayApiClient _apiClient;
        private readonly IUserSessionService _sessionService;

        public EventsController(IRaceDayApiClient apiClient, IUserSessionService sessionService)
        {
            _apiClient = apiClient;
            _sessionService = sessionService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search = null, string? location = null, string? eventType = null)
        {
            var events = await _apiClient.GetEventsAsync(search, location, eventType);

            var filterModel = new EventFilterViewModel
            {
                Search = search,
                Location = location,
                EventType = eventType,
                Events = events
            };

            return View(filterModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var ev = await _apiClient.GetEventByIdAsync(id);
            if (ev == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction(nameof(Index));
            }

            // Also check categories
            if (!ev.Categories.Any())
            {
                ev.Categories = await _apiClient.GetCategoriesAsync(id);
            }

            var model = new EventDetailViewModel
            {
                Event = ev,
                UserRole = _sessionService.Role
            };

            // Check if current participant is already enrolled
            if (_sessionService.IsAuthenticated && _sessionService.IsParticipant && _sessionService.UserId.HasValue)
            {
                var myEnrolments = await _apiClient.GetMyEnrolmentsAsync(_sessionService.UserId.Value, _sessionService.Token);
                var existing = myEnrolments.FirstOrDefault(en => en.EventId == id);
                if (existing != null)
                {
                    model.IsUserEnrolled = true;
                    model.EnrolledCategoryId = existing.CategoryId;
                    model.UserEnrolmentStatus = existing.PaymentStatus;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            if (!_sessionService.IsAuthenticated || !_sessionService.UserId.HasValue)
            {
                TempData["WarningMessage"] = "Please log in to enrol in a race event.";
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Events/Details/{eventId}" });
            }

            if (!_sessionService.IsParticipant && !_sessionService.IsOrganiser)
            {
                TempData["ErrorMessage"] = "Only registered participants can enrol for events.";
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            var success = await _apiClient.EnrolAsync(categoryId, _sessionService.UserId.Value, _sessionService.Token);
            if (success)
            {
                TempData["SuccessMessage"] = "Congratulations! You have successfully enrolled for this event.";
                return RedirectToAction("MyEnrolments", "Participant");
            }

            TempData["ErrorMessage"] = "Enrolment failed. You may already be enrolled in this category or the event category is fully booked.";
            return RedirectToAction(nameof(Details), new { id = eventId });
        }
    }
}
