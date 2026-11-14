using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly RaceDayApiService _apiService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(RaceDayApiService apiService, ILogger<HomeController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Calls your API to get upcoming events for the home page
                // (Make sure your API has an events endpoint implemented from Part 2)
                var events = await _apiService.GetUpcomingEventsAsync();
                return View(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching events from API.");
                // Fallback to empty list so the page doesn't crash
                return View(new List<object>());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}