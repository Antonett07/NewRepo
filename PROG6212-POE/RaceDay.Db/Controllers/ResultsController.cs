using Microsoft.AspNetCore.Mvc;

namespace RaceDay.DB.Controllers
{
    public class ResultsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
