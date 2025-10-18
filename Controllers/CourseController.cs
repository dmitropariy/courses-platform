using Microsoft.AspNetCore.Mvc;

namespace courses_platform.Controllers
{
    public class CourseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
