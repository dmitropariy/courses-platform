using Microsoft.EntityFrameworkCore;
using courses_platform.Models;
using System;
using courses_platform.Contexts;

namespace courses_platform
{
    public static class SeedData
    {

        private static DateTime UtcNow() => DateTime.UtcNow;
        private static DateTime? Utc(DateTime? dt) => dt.HasValue
            ? DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc)
            : null;
        public static void Initialize(ApplicationDbContext context)
        {
            if (context.GetType().Name.Contains("InMemory", StringComparison.OrdinalIgnoreCase))
                return;

            context.Database.Migrate();

            if (context.Courses.Any()) return; // Already seeded

            var random = new Random();

            // === 1. Courses ===
            var courses = new Course[]
            {
                new() { Title = "Вступ до програмування на C#", Description = "Базовий курс для початківців" },
                new() { Title = "Алгоритми та структури даних", Description = "Теорія та практика" },
                new() { Title = "Веб-розробка на ASP.NET Core", Description = "Повний стек MVC + EF Core" }
            };
            context.Courses.AddRange(courses);
            context.SaveChanges();

            // === 2. Modules ===
            var modules = new List<Module>();
            int moduleOrder = 1;
            foreach (var course in courses)
            {
                for (int i = 1; i <= 3; i++)
                {
                    var module = new Module
                    {
                        Course = course,
                        Title = course.CourseId == 1
                            ? i == 1 ? "Змінні та типи даних"
                            : i == 2 ? "Умовні оператори та цикли"
                            : "Масиви та функції"
                            : course.CourseId == 2
                            ? i == 1 ? "Складність алгоритмів"
                            : i == 2 ? "Списки та стеки"
                            : "Дерева та графи"
                            : i == 1 ? "MVC архітектура"
                            : i == 2 ? "Entity Framework Core"
                            : "API та автентифікація",
                        ModuleDescription = "Модуль " + moduleOrder,
                        OrderNumber = moduleOrder++
                    };
                    modules.Add(module);
                    context.Modules.Add(module);
                }
            }
            context.SaveChanges();

            //// === 3. Lessons ===
            var lessons = new List<Lesson>();
            int lessonOrder = 1;
            foreach (var module in modules)
            {
                for (int i = 1; i <= 3; i++)
                {
                    var lesson = new Lesson
                    {
                        Module = module,
                        Title = $"Урок {i}: {GetLessonTitle(module, i)}",
                        LessonDescription = "Детальний розбір теми з прикладами коду.",
                        OrderNumber = lessonOrder++
                    };
                    lessons.Add(lesson);
                    context.Lessons.Add(lesson);

                    // Add content blocks
                    context.LessonContentBlocks.AddRange(
                        new LessonContentBlock { Lesson = lesson, BlockType = "text", Content = $"Це текстовий блок уроку {i}.", Order = 1 },
                        new LessonContentBlock { Lesson = lesson, BlockType = "image", MediaUrl = $"https://res.cloudinary.com/dxtnc9hfj/raw/upload/v1762634083/uiv1rww16xmktex5xdqz.png", Order = 2 },
                        new LessonContentBlock { Lesson = lesson, BlockType = "video", MediaUrl = $"https://res.cloudinary.com/dxtnc9hfj/raw/upload/v1762634083/uiv1rww16xmktex5xdqz.png", Order = 3 }
                    );
                }
            }
            context.SaveChanges();

            // === 4. Assignments ===
            var assignments = new List<Assignment>();
            foreach (var module in modules)
            {
                assignments.AddRange(new[]
                {
                    new Assignment
                    {
                        Module = module,
                        Title = "Тест: Основи синтаксису",
                        Type = "quiz_single",
                        QuestionText = "Який тип даних використовується для зберігання цілого числа?"
                    },
                    new Assignment
                    {
                        Module = module,
                        Title = "Відкрите питання",
                        Type = "open_text",
                        QuestionText = "Поясніть різницю між `stack` та `heap`."
                    }
                });
            }
            context.Assignments.AddRange(assignments);
            context.SaveChanges();

            // === 5. AssignmentOptions  ===
            foreach (var assignment in assignments.Where(a => a.Type.Contains("quiz")))
            {
                var options = assignment.Type == "quiz_single"
                    ? new[] { "int", "string", "bool", "double" }
                    : new[] { "List<T>", "Array", "Dictionary", "Stack" };

                for (int i = 0; i < options.Length; i++)
                {
                    context.AssignmentOptions.Add(new AssignmentOption
                    {
                        Assignment = assignment,
                        Text = options[i],
                        IsCorrect = i == 0 
                    });
                }
            }
            context.SaveChanges();

            // === 6. Submissions ===

            var gradedDates = Enumerable.Range(0, 30)
                .Select(i => UtcNow().AddDays(-random.Next(1, 60)).AddHours(-random.Next(0, 24)))
                .ToList();

            var answers = new string[]{ "answer", "annaISwear", "annaAmmaCriminal", "manna", "bruh" };

            int submissionId = 1;
            foreach (var assignment in assignments.Take(15)) 
            {
                for (int i = 0; i < 2; i++)
                {
                    var answerTextIdx = random.Next(1, 5);
                    var isQuiz = assignment.Type.Contains("quiz");
                    context.Submissions.Add(new Submission
                    {
                        Assignment = assignment,
                        AnswerText = isQuiz ? null : answers[answerTextIdx],
                        SelectedOptions = isQuiz ? "1" : null,
                        IsCorrect = isQuiz ? (i % 2 == 0) : (bool?)null,
                        GradedAt = gradedDates[random.Next(gradedDates.Count)]
                    });
                    submissionId++;
                }
            }
            context.SaveChanges();

            // === 7. CourseVerifications ===
            var verificationStatuses = new[] { "pending", "approved", "rejected" };

            foreach (var course in courses)
            {
                var count = random.Next(1, 4);
                for (int i = 0; i < count; i++)
                {
                    var status = verificationStatuses[random.Next(verificationStatuses.Length)];
                    var comment = status switch
                    {
                        "approved" => "Курс відповідає стандартам платформи.",
                        "rejected" => "Знайдено помилки в матеріалах.",
                        _ => null
                    };

                    context.CourseVerifications.Add(new CourseVerification
                    {
                        CourseId = course.CourseId, 
                        Status = status,
                        ReviewComment = comment,
                        VerifiedAt = status != "pending"
                            ? UtcNow().AddDays(-random.Next(1, 30))
                            : null
                    });
                }
            }
            context.SaveChanges();
        }

        private static string GetLessonTitle(Module module, int lessonNum)
        {
            return module.Title switch
            {
                "Змінні та типи даних" => lessonNum == 1 ? "int, string, bool" : lessonNum == 2 ? "var та dynamic" : "Константи",
                "Умовні оператори та цикли" => lessonNum == 1 ? "if-else" : lessonNum == 2 ? "for, foreach" : "while, do-while",
                "Масиви та функції" => lessonNum == 1 ? "Одновимірні масиви" : lessonNum == 2 ? "Методи" : "Параметри ref/out",
                _ => $"Тема {lessonNum}"
            };
        }
    }
}
