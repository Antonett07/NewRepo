using Microsoft.AspNetCore.Mvc;

namespace RaceDay.DB.Controllers
{
    public class EnrolmentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
