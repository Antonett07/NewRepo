using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [AuthorizeRole("Participant", "Organiser")]
    public class ParticipantController : Controller
    {
        private readonly IRaceDayApiClient _apiClient;
        private readonly IUserSessionService _sessionService;

        public ParticipantController(IRaceDayApiClient apiClient, IUserSessionService sessionService)
        {
            _apiClient = apiClient;
            _sessionService = sessionService;
        }

        private int CurrentParticipantId => _sessionService.UserId ?? 0;

        // 1. My Enrolments: personal view of all events enrolled and status
        [HttpGet]
        public async Task<IActionResult> MyEnrolments()
        {
            if (CurrentParticipantId <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var enrolments = await _apiClient.GetMyEnrolmentsAsync(CurrentParticipantId, _sessionService.Token);
            return View(enrolments);
        }

        // 2. My Results: personal race history across all completed events
        // Displays event name, date, category, recorded finish time, and finishing position (e.g. 47th out of 312 finishers)
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            if (CurrentParticipantId <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var results = await _apiClient.GetMyResultsAsync(CurrentParticipantId, _sessionService.Token);
            return View(results);
        }

        // 3. Profile shortcut
        [HttpGet]
        public IActionResult Profile()
        {
            return RedirectToAction("Profile", "Account");
        }
    }
}
