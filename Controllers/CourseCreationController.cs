using Microsoft.AspNetCore.Mvc;
using courses_platform.Models;
using Microsoft.EntityFrameworkCore;

namespace courses_platform.Controllers
{
    public class CourseCreationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseCreationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------- Створення курсу
        public IActionResult CreateCourse()
        {
            var course = new Course();
            return View(course);
        }

        [HttpPost]
        public IActionResult CreateCourse(Course course)
        {
            if (ModelState.IsValid)
            {
                course.CompletedCount = 0;
                _context.Courses.Add(course);
                _context.SaveChanges();

                return RedirectToAction("CreateModules", "CourseCreation", new { courseId = course.CourseId });
            }

            return View(course);
        }

        // ---------- Сторінка створення модулів
        public IActionResult CreateModules(int courseId)
        {
            var course = _context.Courses
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
                        .ThenInclude(l => l.LessonContentBlocks)
                .FirstOrDefault(c => c.CourseId == courseId);

            if (course == null) return NotFound();

            return View(course);
        }

        // ---------- Додавання модуля ----------
        [HttpPost]
        public IActionResult AddModule(int courseId, string title, string description)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Назва модуля обов'язкова");

            // Обчислення порядку
            int nextOrder = 1;
            var lastModule = _context.Modules
                .Where(m => m.CourseId == courseId)
                .OrderByDescending(m => m.OrderNumber)
                .FirstOrDefault();
            if (lastModule != null) nextOrder = lastModule.OrderNumber + 1;

            var module = new Module
            {
                CourseId = courseId,
                Title = title,
                ModuleDescription = description,
                OrderNumber = nextOrder
            };

            _context.Modules.Add(module);
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                moduleId = module.ModuleId,
                title = module.Title,
                description = module.ModuleDescription,
                orderNumber = module.OrderNumber
            });
        }

        // ---------- Додавання уроку ----------
        [HttpPost]
        public IActionResult AddLesson(int moduleId, string title, string lessonDescription)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Назва уроку обов'язкова");

            // Обчислення порядку
            int nextOrder = 1;
            var lastLesson = _context.Lessons
                .Where(l => l.ModuleId == moduleId)
                .OrderByDescending(l => l.OrderNumber)
                .FirstOrDefault();
            if (lastLesson != null) nextOrder = lastLesson.OrderNumber + 1;

            var lesson = new Lesson
            {
                ModuleId = moduleId,
                Title = title,
                LessonDescription = lessonDescription,
                OrderNumber = nextOrder
            };

            _context.Lessons.Add(lesson);
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                lessonId = lesson.LessonId,
                title = lesson.Title,
                description = lesson.LessonDescription,
                orderNumber = lesson.OrderNumber
            });
        }

        // GET: сторінка заповнення матеріалу уроку
        public IActionResult CreateLessonContentBlocks(int lessonId)
        {
            var lesson = _context.Lessons
                .Include(l => l.Module)
                .Include(l => l.LessonContentBlocks)
                .FirstOrDefault(l => l.LessonId == lessonId);

            if (lesson == null) return NotFound();

            return View(lesson);
        }

        // POST: додавання нового контент-блоку
        [HttpPost]
        public IActionResult AddLessonContentBlock(int lessonId, string blockType, string content, IFormFile? mediaFile)
        {
            var lesson = _context.Lessons
                .Include(l => l.Module)
                .Include(l => l.LessonContentBlocks)
                .FirstOrDefault(l => l.LessonId == lessonId);

            if (lesson == null) return BadRequest("Урок не знайдено");

            int nextOrder = 1;
            if (lesson.LessonContentBlocks.Any())
                nextOrder = lesson.LessonContentBlocks.Max(b => b.Order) + 1;

            string? mediaUrl = null;

            // Якщо користувач завантажив файл
            if (mediaFile != null && mediaFile.Length > 0)
            {
                // Створюємо папку wwwroot/uploads якщо не існує
                string uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                // Генеруємо унікальне ім'я файлу
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(mediaFile.FileName);
                string filePath = Path.Combine(uploadsPath, fileName);

                // Зберігаємо файл
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    mediaFile.CopyTo(stream);
                }

                // Шлях для браузера
                mediaUrl = "/uploads/" + fileName;
            }

            var block = new LessonContentBlock
            {
                LessonId = lessonId,
                BlockType = blockType,
                Content = blockType == "text" ? content : null,
                MediaUrl = mediaUrl,
                Order = nextOrder
            };

            _context.LessonContentBlocks.Add(block);
            _context.SaveChanges();

            return RedirectToAction("CreateLessonContentBlocks", new { lessonId = lessonId });
        }

        // GET: редагування блоку
        public IActionResult EditLessonContentBlock(int blockId)
        {
            var block = _context.LessonContentBlocks
                .Include(b => b.Lesson)
                .FirstOrDefault(b => b.LessonContentBlockId == blockId);

            if (block == null) return NotFound();

            return View(block);
        }

        // POST: збереження редагованого блоку
        [HttpPost]
        public IActionResult EditLessonContentBlock(int lessonContentBlockId, int lessonId, string blockType, string? content, IFormFile? mediaFile)
        {
            var block = _context.LessonContentBlocks.FirstOrDefault(b => b.LessonContentBlockId == lessonContentBlockId);
            if (block == null) return NotFound();

            // Оновлюємо тип контенту
            block.BlockType = blockType;

            // Якщо тип текст — оновлюємо текст
            if (blockType == "text")
            {
                block.Content = content;
                block.MediaUrl = null;
            }
            else
            {
                block.Content = null;

                // Якщо завантажено новий файл — зберігаємо його
                if (mediaFile != null && mediaFile.Length > 0)
                {
                    string uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsPath))
                        Directory.CreateDirectory(uploadsPath);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(mediaFile.FileName);
                    string filePath = Path.Combine(uploadsPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        mediaFile.CopyTo(stream);
                    }

                    block.MediaUrl = "/uploads/" + fileName;
                }
            }

            _context.LessonContentBlocks.Update(block);
            _context.SaveChanges();

            return RedirectToAction("CreateLessonContentBlocks", new { lessonId });
        }

    }
}
