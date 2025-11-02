using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using courses_platform.Models;
using Microsoft.AspNetCore.Authorization;

namespace courses_platform.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ЛАБ 3: Додати пошук за автором курсу
        // GET: Список всіх курсів
        [HttpGet]
        [Authorize(Roles = "Student, Professor")]
        public IActionResult Index(string search, int page = 1)
        {
            var query = _context.Courses
                .Include(c => c.Verifications)
                .Where(c => c.Verifications.Any(v => v.Status == "approved"))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(c =>
                    c.Title.ToLower().Contains(search));
            }

            var totalCourses = query.Count();
            var courses = query
                .OrderBy(c => c.Title)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCourses / (double)PageSize);
            ViewBag.Search = search;

            return View(courses);
        }
    }
}
