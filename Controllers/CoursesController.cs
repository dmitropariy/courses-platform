using Microsoft.AspNetCore.Mvc;

namespace courses_platform.Controllers
{
    public class CoursesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
