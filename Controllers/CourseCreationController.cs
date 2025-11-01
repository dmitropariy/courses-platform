using Microsoft.AspNetCore.Mvc;
using courses_platform.Models;
using Microsoft.EntityFrameworkCore;
using courses_platform.Services;
using Microsoft.AspNetCore.Authorization;

namespace courses_platform.Controllers
{
    public class CourseCreationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CloudinaryService _cloudinary;
        public CourseCreationController(ApplicationDbContext context, CloudinaryService cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        // GET: Сторінка створення курсу
        [HttpGet]
        [Authorize(Roles = "Professor")]
        public IActionResult CreateCourse()
        {
            var course = new Course();
            return View(course);
        }

        // POST: Створення курсу
        [HttpPost]
        [Authorize(Roles = "Professor")]
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

        // GET: Сторінка створення модулів
        [HttpGet]
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

        // POST: Створення модуля
        [HttpPost]
        public IActionResult AddModule(int courseId, string title, string description)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Назва модуля обов'язкова");

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

        // POST: Створення уроку
        [HttpPost]
        public IActionResult AddLesson(int moduleId, string title, string lessonDescription)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Назва уроку обов'язкова");

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
        [HttpGet]
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
        public async Task<IActionResult> AddLessonContentBlock(int lessonId, string blockType, string content, IFormFile? mediaFile)
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

            if (mediaFile != null && mediaFile.Length > 0)
            {
                mediaUrl = await _cloudinary.UploadFileAsync(mediaFile);
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
        [HttpGet]
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
        public async Task<IActionResult> EditLessonContentBlock(int lessonContentBlockId, int lessonId, string blockType, string? content, IFormFile? mediaFile)
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

                if (mediaFile != null && mediaFile.Length > 0)
                {
                    block.MediaUrl = await _cloudinary.UploadFileAsync(mediaFile);
                }
            }

            _context.LessonContentBlocks.Update(block);
            _context.SaveChanges();

            return RedirectToAction("CreateLessonContentBlocks", new { lessonId });
        }

        // GET: сторінка створення тестових питань модуля
        [HttpGet]
        public IActionResult CreateModuleAssignments(int moduleId)
        {
            var module = _context.Modules
                .Include(m => m.Assignments)
                    .ThenInclude(a => a.Options)
                .FirstOrDefault(m => m.ModuleId == moduleId);

            if (module == null) return NotFound();
            return View(module);
        }

        // POST: додавання питання до модуля
        [HttpPost]
        public IActionResult AddAssignment(
         int moduleId,
         string title,
         string type,
         string questionText,
         List<string> optionCorrect,
         List<string> optionTexts,
         string openTextAnswer)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError("", "Вкажіть назву питання");
                return RedirectToAction("CreateModuleAssignments", new { moduleId });
            }

            var assignment = new Assignment
            {
                ModuleId = moduleId,
                Title = title,
                Type = type,
                QuestionText = questionText,
                Options = new List<AssignmentOption>()
            };

            if (type == "open_text")
            {
                if (!string.IsNullOrWhiteSpace(openTextAnswer))
                {
                    assignment.Options.Add(new AssignmentOption
                    {
                        Text = openTextAnswer,
                        IsCorrect = true
                    });
                }
            }
            else
            {
                int count = Math.Min(optionTexts.Count, optionCorrect.Count);
                for (int i = 0; i < count; i++)
                {
                    assignment.Options.Add(new AssignmentOption
                    {
                        Text = optionTexts[i],
                        IsCorrect = optionCorrect[i] == "true"
                    });
                }
            }

            _context.Assignments.Add(assignment);
            _context.SaveChanges();

            return RedirectToAction("CreateModuleAssignments", new { moduleId });
        }


        // GET: редагування питання
        [HttpGet]
        public IActionResult EditAssignment(int assignmentId)
        {
            var assignment = _context.Assignments
                .Include(a => a.Options)
                .FirstOrDefault(a => a.AssignmentId == assignmentId);

            if (assignment == null) return NotFound();
            return View(assignment);
        }

        // POST: редагування питання
        [HttpPost]
        public IActionResult EditAssignment(Assignment model, List<string> optionTexts, List<string> optionCorrect, string openTextAnswer)
        {
            var assignment = _context.Assignments
                .Include(a => a.Options)
                .FirstOrDefault(a => a.AssignmentId == model.AssignmentId);
            if (assignment == null) return NotFound();

            assignment.Title = model.Title;
            assignment.Type = model.Type;
            assignment.QuestionText = model.QuestionText;

            _context.AssignmentOptions.RemoveRange(assignment.Options);
            _context.SaveChanges();

            if (model.Type == "open_text")
            {
                _context.AssignmentOptions.Add(new AssignmentOption
                {
                    AssignmentId = assignment.AssignmentId,
                    Text = openTextAnswer,
                    IsCorrect = true
                });
            }
            else if (optionTexts != null && optionTexts.Any())
            {
                for (int i = 0; i < optionTexts.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(optionTexts[i]))
                    {
                        bool isCorrect = false;
                        if (optionCorrect != null && optionCorrect.Count > i)
                        {
                            isCorrect = optionCorrect[i] == "true";
                        }

                        _context.AssignmentOptions.Add(new AssignmentOption
                        {
                            AssignmentId = assignment.AssignmentId,
                            Text = optionTexts[i],
                            IsCorrect = isCorrect
                        });
                    }
                }

            }

            _context.SaveChanges();
            return RedirectToAction("CreateModuleAssignments", new { moduleId = assignment.ModuleId });
        }

        // POST: Видалення питання
        [HttpPost]
        public IActionResult DeleteAssignment(int assignmentId, int moduleId)
        {
            var assignment = _context.Assignments
                .Include(a => a.Options)
                .FirstOrDefault(a => a.AssignmentId == assignmentId);

            if (assignment != null)
            {
                _context.AssignmentOptions.RemoveRange(assignment.Options);
                _context.Assignments.Remove(assignment);
                _context.SaveChanges();
            }

            return RedirectToAction("CreateModuleAssignments", new { moduleId });
        }

        // POST: Надіслання курсу на верифікацію
        [HttpPost]
        public IActionResult SubmitCourse(int courseId)
        {
            var course = _context.Courses.FirstOrDefault(c => c.CourseId == courseId);
            if (course == null)
            {
                return NotFound();
            }

            var existingVerification = _context.CourseVerifications
                .FirstOrDefault(v => v.CourseId == courseId);

            if (existingVerification != null)
            {
                TempData["Message"] = "Курс вже надіслано на перевірку.";
                return RedirectToAction("Index", "Home");
            }

            var verification = new CourseVerification
            {
                CourseId = courseId,
                Status = "pending",
                VerifiedAt = null,
                ReviewComment = ""
            };

            _context.CourseVerifications.Add(verification);
            _context.SaveChanges();

            TempData["Message"] = "Заявка на публікацію курсу надіслана.";
            return RedirectToAction("Index", "Home");
        }

    }
}
