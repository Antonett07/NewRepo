using Microsoft.AspNetCore.Mvc;

namespace RaceDay.DB.Controllers
{
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
