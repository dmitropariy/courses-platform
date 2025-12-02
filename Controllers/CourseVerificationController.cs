using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Authorization;
using courses_platform.Contexts;

public class CourseVerificationController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 10;

    public CourseVerificationController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult VerificationPanel()
    {
        return View();
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult VerificationPanelAjax(string search = "", int page = 1)
    {
        var query = _context.CourseVerifications
            .Include(v => v.Course)
            .Where(v => v.Status != "approved")
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(v =>
                v.Course.Title.ToLower().Contains(search) ||
                v.VerificationId.ToString() == search);
        }

        var totalCount = query.Count();
        var verifications = query
            .OrderByDescending(v => v.VerifiedAt)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(v => new {
                verificationId = v.VerificationId,
                courseTitle = v.Course.Title
            })
            .ToList();

        return Json(new
        {
            items = verifications,
            currentPage = page,
            totalPages = (int)Math.Ceiling(totalCount / (double)PageSize)
        });
    }


    // GET: Деталі конкретної заявки
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult CourseDetails(int verificationId)
    {
        var verification = _context.CourseVerifications
            .Include(v => v.Course)
            .ThenInclude(c => c.Modules)
            .FirstOrDefault(v => v.VerificationId == verificationId);

        if (verification == null) return NotFound();

        return View(verification);
    }

    // POST: Прийняти заявку
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult ApproveCourse(int verificationId)
    {
        var verification = _context.CourseVerifications
            .FirstOrDefault(v => v.VerificationId == verificationId);

        if (verification == null) return NotFound();

        verification.Status = "approved";
        verification.VerifiedAt = DateTime.Now;
        _context.SaveChanges();

        return RedirectToAction("VerificationPanel", "CourseVerification");
    }

    // POST: Відхилити заявку
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult RejectCourse(int verificationId)
    {
        var verification = _context.CourseVerifications
            .FirstOrDefault(v => v.VerificationId == verificationId);

        if (verification == null) return NotFound();

        verification.Status = "rejected";
        verification.VerifiedAt = DateTime.Now;
        _context.SaveChanges();


        return RedirectToAction("VerificationPanel", "CourseVerification");
    }

    // GET: Перегляд деталей модуля (Уроки + Тестове завдання)
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult ModuleDetails(int moduleId)
    {
        var module = _context.Modules
            .Include(m => m.Lessons)
            .FirstOrDefault(m => m.ModuleId == moduleId);

        if (module == null)
        {
            return NotFound();
        }

        module.Lessons = module.Lessons.OrderBy(l => l.OrderNumber).ToList();

        return View(module);
    }

    // GET: Перегляд тестового завдання модуля
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult ModuleAssignments(int moduleId)
    {
        var module = _context.Modules
            .Include(m => m.Lessons)
            .Include(m => m.Assignments)
                .ThenInclude(a => a.Options)
            .FirstOrDefault(m => m.ModuleId == moduleId);

        if (module == null)
            return NotFound();

        return View(module);
    }

    // GET: Перегляд деталей уроку (КонтентБлоки)
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult LessonDetails(int lessonId)
    {
        var lesson = _context.Lessons
            .Include(l => l.LessonContentBlocks)
            .FirstOrDefault(l => l.LessonId == lessonId);

        if (lesson == null)
            return NotFound();

        return View(lesson);
    }


}
