using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using courses_platform.Controllers;
using courses_platform.Models;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace courses_platform.Tests.Controllers
{
    public class CourseCreationControllerTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            // Створимо урок і блок
            var course = new Course { CourseId = 1, Title = "C# Basics", Description = "Desc1" };
            context.Courses.Add(course);
            var module = new Module { ModuleId = 1, CourseId = 1, Title = "Module 1", ModuleDescription = "ModuleDesc1", OrderNumber = 1 };
            context.Modules.Add(module);
            var lesson = new Lesson { LessonId = 1, ModuleId = 1, Title = "Lesson 1", LessonDescription = "LessonDesc1", OrderNumber = 1 };
            context.Lessons.Add(lesson);

            var block = new LessonContentBlock
            {
                LessonContentBlockId = 1,
                LessonId = 1,
                BlockType = "text",
                Content = "Original text",
                Order = 1
            };
            context.LessonContentBlocks.Add(block);

            context.SaveChanges();
            return context;
        }

        [Fact]
        public void CreateCourse_Get_ReturnsView()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var result = controller.CreateCourse();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsAssignableFrom<Course>(viewResult.Model);
        }

        [Fact]
        public void CreateCourse_Post_ValidModel_AddsCourseAndRedirects()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var course = new Course
            {
                Title = "Python Intro",
                Description = "Test course"
            };

            var result = controller.CreateCourse(course);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CreateModules", redirectResult.ActionName);
            Assert.Equal("CourseCreation", redirectResult.ControllerName);

            var savedCourse = context.Courses.First(c => c.Title == "Python Intro");
            Assert.NotNull(savedCourse);
            Assert.Equal(0, savedCourse.CompletedCount);
        }

        [Fact]
        public void CreateCourse_Post_InvalidModel_ReturnsViewWithModel()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);
            controller.ModelState.AddModelError("Title", "Required");

            var courses_before_adding = context.Courses.Count();

            var course = new Course
            {
                Description = "Test course"
            };

            var result = controller.CreateCourse(course);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(course, viewResult.Model);
            Assert.Equal(courses_before_adding, context.Courses.Count());
        }

        [Fact]
        public void CreateModules_Get_ReturnsViewWithCourse()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var result = controller.CreateModules(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Course>(viewResult.Model);
            Assert.Equal(1, model.CourseId);
        }

        [Fact]
        public void AddModule_Post_AddsModule()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var result = controller.AddModule(1, "Module 2", "Description 2");

            // Перевіряємо, що модуль додано у базу
            var module = context.Modules.FirstOrDefault(m => m.ModuleDescription == "Description 2");
            Assert.NotNull(module);
            Assert.Equal("Module 2", module.Title);
            Assert.Equal(2, module.OrderNumber);
        }

        [Fact]
        public void AddModule_Post_EmptyTitle_ReturnsBadRequest()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var modules_before_adding = context.Modules.Count();

            var result = controller.AddModule(1, "", "Desc");

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(modules_before_adding, context.Modules.Count());
        }

        [Fact]
        public void AddLesson_Post_AddsLessonToModule()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var result = controller.AddLesson(1, "Lesson 2", "Lesson Desc 2");

            var lesson = context.Lessons.FirstOrDefault(l => l.LessonDescription == "Lesson Desc 2");
            Assert.NotNull(lesson);
            Assert.Equal("Lesson 2", lesson.Title);
            Assert.Equal("Lesson Desc 2", lesson.LessonDescription);
            Assert.Equal(2, lesson.OrderNumber);
            Assert.Equal(1, lesson.ModuleId);
        }

        [Fact]
        public void AddLesson_Post_EmptyTitle_ReturnsBadRequest()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var lessons_before_adding = context.Lessons.Count();

            var result = controller.AddLesson(1, "", "Lesson Desc");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Назва уроку обов'язкова", badRequestResult.Value);
            Assert.Equal(lessons_before_adding, context.Lessons.Count());
        }

        [Fact]
        public void CreateLessonContentBlocks_Get_ReturnsViewWithLesson()
        { 
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var result = controller.CreateLessonContentBlocks(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Lesson>(viewResult.Model);
            Assert.Equal(1, model.LessonId);
            Assert.Equal("Lesson 1", model.Title);
        }

        [Fact]
        public async void AddLessonContentBlock_Post_AddsTextBlock()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var result = await controller.AddLessonContentBlock(1, "text", "New block content", null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CreateLessonContentBlocks", redirect.ActionName);

            var block = context.LessonContentBlocks.FirstOrDefault(b => b.Content == "New block content");
            Assert.NotNull(block);
            Assert.Equal("text", block.BlockType);
            Assert.Null(block.MediaUrl);
        }

        [Fact]
        public async void AddLessonContentBlock_Post_InvalidLesson_ReturnsBadRequest()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var result = await controller.AddLessonContentBlock(88, "text", "Content", null);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Урок не знайдено", badRequestResult.Value);
        }

        [Fact]
        public void EditLessonContentBlock_Get_ReturnsViewWithBlock()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var result = controller.EditLessonContentBlock(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<LessonContentBlock>(viewResult.Model);
            Assert.Equal(1, model.LessonContentBlockId);
            Assert.Equal("Original text", model.Content);
        }

        [Fact]
        public void EditLessonContentBlock_Get_BlockNotFound_ReturnsNotFound()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var result = controller.EditLessonContentBlock(88);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async void EditLessonContentBlock_Post_UpdatesTextBlock()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var result = await controller.EditLessonContentBlock(1, 1, "text", "Updated text", null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CreateLessonContentBlocks", redirect.ActionName);

            var block = context.LessonContentBlocks.First();
            Assert.Equal("text", block.BlockType);
            Assert.Equal("Updated text", block.Content);
            Assert.Null(block.MediaUrl);
        }

        [Fact]
        public async void EditLessonContentBlock_Post_BlockNotFound_ReturnsNotFound()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var result = await controller.EditLessonContentBlock(88, 1, "text", "Updated", null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void CreateModuleAssignments_Get_ReturnsViewWithModule()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var result = controller.CreateModuleAssignments(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Module>(viewResult.Model);
            Assert.Equal(1, model.ModuleId);
        }

        [Fact]
        public void CreateModuleAssignments_Get_ModuleNotFound_ReturnsNotFound()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var result = controller.CreateModuleAssignments(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void AddAssignment_Post_AddsOpenTextAssignment()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var result = controller.AddAssignment(
                moduleId: 1,
                title: "Open question",
                type: "open_text",
                questionText: "What is C#?",
                optionCorrect: null,
                optionTexts: null,
                openTextAnswer: "C# is a language"
            );

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CreateModuleAssignments", redirect.ActionName);
            Assert.Equal(1, redirect.RouteValues["moduleId"]);

            var assignment = context.Assignments.Include(a => a.Options).FirstOrDefault(a => a.Title == "Open question");
            Assert.NotNull(assignment);
            Assert.Single(assignment.Options);
            Assert.Equal("C# is a language", assignment.Options.First().Text);
            Assert.True(assignment.Options.First().IsCorrect);
        }

        [Fact]
        public void AddAssignment_Post_AddsMultipleChoiceAssignment()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var result = controller.AddAssignment(
                moduleId: 1,
                title: "MCQ",
                type: "multiple_choice",
                questionText: "Choose correct options",
                optionCorrect: new List<string> { "true", "false", "true" },
                optionTexts: new List<string> { "Option 1", "Option 2", "Option 3" },
                openTextAnswer: null
            );

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CreateModuleAssignments", redirect.ActionName);

            var assignment = context.Assignments.Include(a => a.Options).FirstOrDefault(a => a.Title == "MCQ");
            Assert.NotNull(assignment);
            var options = assignment.Options.ToList();
            Assert.Equal(3, options.Count);
            Assert.True(options[0].IsCorrect);
            Assert.False(options[1].IsCorrect);
            Assert.True(options[2].IsCorrect);

        }

        [Fact]
        public void AddAssignment_Post_EmptyTitle_RedirectsWithoutAdding()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var assignmentsBefore = context.Assignments.Count();

            var result = controller.AddAssignment(
                moduleId: 1,
                title: "",
                type: "open_text",
                questionText: "Q",
                optionCorrect: null,
                optionTexts: null,
                openTextAnswer: "Answer"
            );

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CreateModuleAssignments", redirect.ActionName);
            Assert.Equal(1, redirect.RouteValues["moduleId"]);

            Assert.Equal(assignmentsBefore, context.Assignments.Count());
        }

        [Fact]
        public void EditAssignment_Get_ReturnsViewWithAssignment()
        {
            var context = GetDbContext();
            var assignment = new Assignment
            {
                AssignmentId = 1,
                ModuleId = 1,
                Title = "Test assignment",
                Type = "open_text",
                QuestionText = "Q"
            };
            context.Assignments.Add(assignment);
            context.SaveChanges();

            var controller = new CourseCreationController(context, null);

            var result = controller.EditAssignment(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Assignment>(viewResult.Model);
            Assert.Equal(1, model.AssignmentId);
        }

        [Fact]
        public void EditAssignment_Get_AssignmentNotFound_ReturnsNotFound()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var result = controller.EditAssignment(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void EditAssignment_Post_UpdatesOpenTextAssignment()
        {
            var context = GetDbContext();
            var assignment = new Assignment
            {
                AssignmentId = 1,
                ModuleId = 1,
                Title = "Old title",
                Type = "open_text",
                QuestionText = "Old Q",
                Options = new List<AssignmentOption>
        {
            new AssignmentOption { Text = "Old answer", IsCorrect = true }
        }
            };
            context.Assignments.Add(assignment);
            context.SaveChanges();

            var controller = new CourseCreationController(context, null);

            var updatedModel = new Assignment
            {
                AssignmentId = 1,
                Title = "New title",
                Type = "open_text",
                QuestionText = "New Q"
            };

            var result = controller.EditAssignment(updatedModel, null, null, "New answer");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CreateModuleAssignments", redirect.ActionName);

            var updated = context.Assignments.Include(a => a.Options).First();
            Assert.Equal("New title", updated.Title);
            Assert.Equal("New Q", updated.QuestionText);
            Assert.Single(updated.Options);
            Assert.Equal("New answer", updated.Options.First().Text);
            Assert.True(updated.Options.First().IsCorrect);
        }

        [Fact]
        public void DeleteAssignment_Post_RemovesAssignmentAndOptions()
        {
            var context = GetDbContext();
            var assignment = new Assignment
            {
                AssignmentId = 1,
                ModuleId = 1,
                Title = "To delete",
                Type = "open_text",
                QuestionText = "question",
                Options = new List<AssignmentOption>
                {
                    new AssignmentOption { Text = "Answer", IsCorrect = true }
                }
            };
            context.Assignments.Add(assignment);
            context.SaveChanges();

            var controller = new CourseCreationController(context, null);

            var result = controller.DeleteAssignment(1, 1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CreateModuleAssignments", redirect.ActionName);
            Assert.Equal(1, redirect.RouteValues["moduleId"]);

            Assert.Empty(context.Assignments);
            Assert.Empty(context.AssignmentOptions);
        }

        [Fact]
        public void SubmitCourse_Post_CourseNotFound_ReturnsNotFound()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);

            var result = controller.SubmitCourse(999); // курс не існує

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void SubmitCourse_Post_AlreadySubmitted_SetsTempDataAndRedirects()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);
            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );

            // Додамо вже надісланий курс
            context.CourseVerifications.Add(new CourseVerification
            {
                CourseId = 1,
                Status = "pending",
                VerifiedAt = null,
                ReviewComment = ""
            });
            context.SaveChanges();

            var result = controller.SubmitCourse(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);

            Assert.Equal("Курс вже надіслано на перевірку.", controller.TempData["Message"]);
        }

        [Fact]
        public void SubmitCourse_Post_NewCourse_CreatesVerificationAndRedirects()
        {
            var context = GetDbContext();
            var controller = new CourseCreationController(context, null);
            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );

            var result = controller.SubmitCourse(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);

            var verification = context.CourseVerifications.FirstOrDefault(v => v.CourseId == 1);
            Assert.NotNull(verification);
            Assert.Equal("pending", verification.Status);
            Assert.Null(verification.VerifiedAt);
            Assert.Equal("", verification.ReviewComment);

            Assert.Equal("Заявка на публікацію курсу надіслана.", controller.TempData["Message"]);
        }


    }
}
