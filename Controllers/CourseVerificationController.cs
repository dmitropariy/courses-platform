using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using courses_platform.Models;
using System;

public class CourseVerificationController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 10;

    public CourseVerificationController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Список всіх заявок
    public IActionResult Index(string search, int page = 1)
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
            .ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        ViewBag.Search = search;

        return View(verifications);
    }

    // GET: Деталі конкретної заявки
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
    public IActionResult Approve(int verificationId)
    {
        var verification = _context.CourseVerifications
            .FirstOrDefault(v => v.VerificationId == verificationId);

        if (verification == null) return NotFound();

        verification.Status = "approved";
        verification.VerifiedAt = DateTime.Now;
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // POST: Відхилити заявку
    [HttpPost]
    public IActionResult Reject(int verificationId)
    {
        var verification = _context.CourseVerifications
            .FirstOrDefault(v => v.VerificationId == verificationId);

        if (verification == null) return NotFound();

        verification.Status = "rejected";
        verification.VerifiedAt = DateTime.Now;
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // GET: ModuleDetails
    [HttpGet]
    public IActionResult ModuleDetails(int moduleId)
    {
        var module = _context.Modules
            .Include(m => m.Lessons)
            .FirstOrDefault(m => m.ModuleId == moduleId);

        if (module == null)
        {
            return NotFound();
        }

        // Впорядкувати уроки за OrderNumber
        module.Lessons = module.Lessons.OrderBy(l => l.OrderNumber).ToList();

        return View(module);
    }

    // GET: перегляд тестового завдання модуля
    [HttpGet]
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

    // GET: LessonDetails
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
