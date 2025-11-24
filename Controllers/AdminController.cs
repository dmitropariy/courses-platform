using courses_platform.Models;
using courses_platform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using courses_platform.Models.ViewModels;
using courses_platform.Contexts;

namespace courses_platform.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext context)
        {
            _db = context;
        }

        const int PageSize = 10;

        public IActionResult AdminLogin()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Search()
        {
            ViewBag.Courses = _db.Courses
                                 .Select(c => new SelectListItem
                                 {
                                     Value = c.CourseId.ToString(),
                                     Text = c.Title
                                 }).ToList();

            ViewBag.AssignmentTypes = new List<SelectListItem>
            {
                new() { Value = "", Text = "Усі типи" },
                new() { Value = "quiz_single", Text = "Одиночний вибір" },
                new() { Value = "quiz_multiple", Text = "Множинний вибір" },
                new() { Value = "open_text", Text = "Відкрите питання" }
            };

            return View(new SubmissionSearchModel());
        }

        [HttpPost]
        public IActionResult Search(SubmissionSearchModel model, int page = 1)
        {
            ViewBag.Courses = _db.Courses
                                 .Select(c => new SelectListItem
                                 {
                                     Value = c.CourseId.ToString(),
                                     Text = c.Title
                                 }).ToList();
            ViewBag.AssignmentTypes = new List<SelectListItem>
            {
                new() { Value = "", Text = "Усі типи" },
                new() { Value = "quiz_single", Text = "Одиночний вибір" },
                new() { Value = "quiz_multiple", Text = "Множинний вибір" },
                new() { Value = "open_text", Text = "Відкрите питання" }
            };

            IQueryable<Submission> query = _db.Submissions
                .Include(s => s.Assignment)
                .ThenInclude(a => a.Module)
                .ThenInclude(m => m.Course);

            if (model.DateFrom.HasValue)
                query = query.Where(s => s.GradedAt >= model.DateFrom);
            if (model.DateTo.HasValue)
                query = query.Where(s => s.GradedAt <= model.DateTo);

            if (model.SelectedCourseIds != null && model.SelectedCourseIds.Any())
            {
                var courseIds = model.SelectedCourseIds.Select(int.Parse);
                query = query.Where(s => courseIds.Contains(s.Assignment.Module.CourseId));
            }

            if (!string.IsNullOrEmpty(model.AssignmentType))
                query = query.Where(s => s.Assignment.Type == model.AssignmentType);

            if (!string.IsNullOrEmpty(model.AnswerPrefix))
                query = query.Where(s => s.AnswerText != null && s.AnswerText.StartsWith(model.AnswerPrefix));
            if (!string.IsNullOrEmpty(model.AnswerSuffix))
                query = query.Where(s => s.AnswerText != null && s.AnswerText.EndsWith(model.AnswerSuffix));

            var total = query.Count();

            var results = query.OrderByDescending(s => s.GradedAt)
                               .Skip((page - 1) * PageSize)
                               .Take(PageSize)
                               .Select(s => new SubmissionResultViewModel
                               {
                                   SubmissionId = s.SubmissionId,
                                   CourseTitle = s.Assignment.Module.Course.Title,
                                   ModuleTitle = s.Assignment.Module.Title,
                                   AssignmentTitle = s.Assignment.Title,
                                   AnswerText = s.AnswerText ?? s.SelectedOptions ?? "-",
                                   IsCorrect = s.IsCorrect,
                                   GradedAt = s.GradedAt
                               })
                               .ToList();

            ViewBag.Results = results;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (total + PageSize - 1) / PageSize;
            ViewBag.TotalCount = total;

            return View(model);
        }
    }
}
