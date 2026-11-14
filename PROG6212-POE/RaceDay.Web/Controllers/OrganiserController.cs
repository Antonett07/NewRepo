using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers
{
    [AuthorizeRole("Organiser")]
    public class OrganiserController : Controller
    {
        private readonly IRaceDayApiClient _apiClient;
        private readonly IUserSessionService _sessionService;

        public OrganiserController(IRaceDayApiClient apiClient, IUserSessionService sessionService)
        {
            _apiClient = apiClient;
            _sessionService = sessionService;
        }

        private int CurrentOrganiserId => _sessionService.UserId ?? 1;

        // 1. Dashboard: summary of all events created, total enrolments per event, upcoming dates
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var events = await _apiClient.GetOrganiserEventsAsync(CurrentOrganiserId, _sessionService.Token);

            var model = new OrganiserDashboardViewModel
            {
                OrganiserName = _sessionService.FullName ?? "Race Organiser",
                TotalEvents = events.Count,
                TotalEnrolments = events.Sum(e => e.TotalEnrolments),
                UpcomingEventsCount = events.Count(e => e.EventDate >= DateTime.Now),
                CompletedEventsCount = events.Count(e => e.EventDate < DateTime.Now),
                Events = events
            };

            return View(model);
        }

        // 2. Events Management Page: list all events to create, edit, delete
        [HttpGet]
        public async Task<IActionResult> Events()
        {
            var events = await _apiClient.GetOrganiserEventsAsync(CurrentOrganiserId, _sessionService.Token);
            return View(events);
        }

        // 3. Create Event: with banner upload through Part 2 API to Azure Blob
        [HttpGet]
        public IActionResult CreateEvent()
        {
            return View(new CreateEventViewModel
            {
                EventDate = DateTime.Now.AddDays(30)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Upload banner image to Azure Blob Storage via Part 2 API
            if (model.BannerImage != null && model.BannerImage.Length > 0)
            {
                var bannerUrl = await _apiClient.UploadBannerImageAsync(model.BannerImage, _sessionService.Token);
                if (!string.IsNullOrEmpty(bannerUrl))
                {
                    model.ExistingBannerUrl = bannerUrl;
                }
            }

            var success = await _apiClient.CreateEventAsync(model, CurrentOrganiserId, _sessionService.Token);
            if (success)
            {
                TempData["SuccessMessage"] = $"Event '{model.EventName}' created successfully! You can now add categories.";
                return RedirectToAction(nameof(Events));
            }

            ModelState.AddModelError(string.Empty, "Failed to create event. Please ensure all fields are valid.");
            return View(model);
        }

        // 4. Edit Event: with banner upload through Part 2 API to Azure Blob
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var ev = await _apiClient.GetEventByIdAsync(id);
            if (ev == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction(nameof(Events));
            }

            var model = new CreateEventViewModel
            {
                EventId = ev.EventId,
                EventName = ev.EventName,
                Description = ev.Description,
                Location = ev.Location,
                EventDate = ev.EventDate,
                EventType = ev.EventType,
                Distance = ev.Distance,
                ExistingBannerUrl = ev.BannerImageUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, CreateEventViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Upload new banner image if provided
            if (model.BannerImage != null && model.BannerImage.Length > 0)
            {
                var bannerUrl = await _apiClient.UploadBannerImageAsync(model.BannerImage, _sessionService.Token);
                if (!string.IsNullOrEmpty(bannerUrl))
                {
                    model.ExistingBannerUrl = bannerUrl;
                }
            }

            var success = await _apiClient.UpdateEventAsync(id, model, CurrentOrganiserId, _sessionService.Token);
            if (success)
            {
                TempData["SuccessMessage"] = $"Event '{model.EventName}' was updated successfully!";
                return RedirectToAction(nameof(Events));
            }

            ModelState.AddModelError(string.Empty, "Failed to update event. Please try again.");
            return View(model);
        }

        // 5. Delete Event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var success = await _apiClient.DeleteEventAsync(id, CurrentOrganiserId, _sessionService.Token);
            if (success)
            {
                TempData["SuccessMessage"] = "Event was deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete event.";
            }

            return RedirectToAction(nameof(Events));
        }

        // 6. Category Management Page: add age or distance categories to each event
        [HttpGet]
        public async Task<IActionResult> Categories(int eventId)
        {
            var ev = await _apiClient.GetEventByIdAsync(eventId);
            if (ev == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction(nameof(Events));
            }

            var categories = await _apiClient.GetCategoriesAsync(eventId);
            ViewBag.Event = ev;

            var model = new CreateCategoryViewModel
            {
                EventId = eventId,
                EventName = ev.EventName
            };

            ViewBag.Categories = categories;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(CreateCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var ev = await _apiClient.GetEventByIdAsync(model.EventId);
                ViewBag.Event = ev;
                ViewBag.Categories = await _apiClient.GetCategoriesAsync(model.EventId);
                return View("Categories", model);
            }

            var success = await _apiClient.AddCategoryAsync(model.EventId, model, _sessionService.Token);
            if (success)
            {
                TempData["SuccessMessage"] = $"Category '{model.CategoryName}' added successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to add category.";
            }

            return RedirectToAction(nameof(Categories), new { eventId = model.EventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id, int eventId)
        {
            var success = await _apiClient.DeleteCategoryAsync(id, _sessionService.Token);
            if (success)
            {
                TempData["SuccessMessage"] = "Category removed successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete category.";
            }

            return RedirectToAction(nameof(Categories), new { eventId });
        }

        // 7. Enrolments Page: view all Participants enrolled for a specific event
        [HttpGet]
        public async Task<IActionResult> Enrolments(int eventId)
        {
            var ev = await _apiClient.GetEventByIdAsync(eventId);
            if (ev == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction(nameof(Events));
            }

            var enrolments = await _apiClient.GetEventEnrolmentsAsync(eventId, _sessionService.Token);
            ViewBag.Event = ev;

            return View(enrolments);
        }

        // 8. Results Entry Page: capture and publish finish times and finishing positions
        [HttpGet]
        public async Task<IActionResult> ResultsEntry(int eventId)
        {
            var ev = await _apiClient.GetEventByIdAsync(eventId);
            if (ev == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction(nameof(Events));
            }

            var enrolments = await _apiClient.GetEventEnrolmentsAsync(eventId, _sessionService.Token);

            var model = new EventResultsEntryViewModel
            {
                EventId = ev.EventId,
                EventName = ev.EventName,
                EventDate = ev.EventDate,
                Location = ev.Location,
                Participants = enrolments.Select(e => new ParticipantResultRowViewModel
                {
                    EnrolmentId = e.EnrolmentId,
                    ParticipantId = e.ParticipantId,
                    ParticipantName = e.ParticipantName,
                    CategoryName = e.CategoryName,
                    RaceNumber = e.RaceNumber,
                    PaymentStatus = e.PaymentStatus,
                    ExistingResultId = e.Result?.ResultId,
                    FinishTime = e.Result?.FinishTime,
                    OverallPosition = e.Result?.OverallPosition,
                    CategoryPosition = e.Result?.CategoryPosition,
                    Status = e.Result?.Status ?? "Finished"
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublishResult(int eventId, CreateResultViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid result inputs. Please ensure finish time is in hh:mm:ss and position is valid.";
                return RedirectToAction(nameof(ResultsEntry), new { eventId });
            }

            var success = await _apiClient.SubmitResultAsync(model, _sessionService.Token);
            if (success)
            {
                TempData["SuccessMessage"] = "Participant result captured and published successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to publish result. Please try again.";
            }

            return RedirectToAction(nameof(ResultsEntry), new { eventId });
        }
    }
}
