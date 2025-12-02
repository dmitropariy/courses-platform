using courses_platform.Contexts;
using courses_platform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace courses_platform.Tests.Controllers
{
    public class CourseVerificationControllerTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);

            context.Courses.AddRange(
                new Course { CourseId = 1, Title = "C# Basics", Description = "Desc1",
                    Modules = {
                        new Module {
                            ModuleId = 1,
                            Title = "Intro",
                            ModuleDescription = "ModuleDesc1",
                            OrderNumber = 1,
                            Lessons = {
                                new Lesson { LessonId = 1, Title = "Lesson1", LessonDescription = "LessonDesc1", OrderNumber = 2 },
                                new Lesson { LessonId = 2, Title = "Lesson2", LessonDescription = "LessonDesc2", OrderNumber = 1 }
                            }
                        }
                    }
                },
                new Course { CourseId = 2, Title = "Python Intro", Description = "Desc1" }
            );

            context.CourseVerifications.AddRange(
                new CourseVerification { VerificationId = 1, Status = "pending", CourseId = 1 },
                new CourseVerification { VerificationId = 2, Status = "approved", CourseId = 2 },
                new CourseVerification { VerificationId = 3, Status = "rejected", CourseId = 1 }
            );

            context.SaveChanges();
            return context;
        }

        [Fact]
        public void CourseDetails_ReturnsView_ForExistingVerification()
        {
            var context = GetDbContext();
            var controller = new CourseVerificationController(context);

            var result = controller.CourseDetails(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<CourseVerification>(viewResult.Model);
            Assert.Equal(1, model.VerificationId);
            Assert.NotNull(model.Course);
        }

        [Fact]
        public void CourseDetails_ReturnsNotFound_ForNonExistingVerification()
        {
            var context = GetDbContext();
            var controller = new CourseVerificationController(context);

            var result = controller.CourseDetails(88);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Approve_UpdatesStatusAndRedirects()
        {
            var context = GetDbContext();
            var controller = new CourseVerificationController(context);

            var result = controller.ApproveCourse(1);

            var verification = context.CourseVerifications.First(v => v.VerificationId == 1);
            Assert.Equal("approved", verification.Status);
            Assert.NotNull(verification.VerifiedAt);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public void Approve_ReturnsNotFound_ForNonExistingVerification()
        {
            var context = GetDbContext();
            var controller = new CourseVerificationController(context);

            var result = controller.ApproveCourse(88);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Reject_UpdatesStatusAndRedirects()
        {
            var context = GetDbContext();
            var controller = new CourseVerificationController(context);

            var result = controller.RejectCourse(3);

            var verification = context.CourseVerifications.First(v => v.VerificationId == 3);
            Assert.Equal("rejected", verification.Status);
            Assert.NotNull(verification.VerifiedAt);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public void Reject_ReturnsNotFound_ForNonExistingVerification()
        {
            var context = GetDbContext();
            var controller = new CourseVerificationController(context);

            var result = controller.RejectCourse(88);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void ModuleDetails_ReturnsView_WithOrderedLessons()
        {
            var context = GetDbContext();
            var controller = new CourseVerificationController(context);

            var result = controller.ModuleDetails(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Module>(viewResult.Model);

            Assert.Equal(1, model.ModuleId);
            Assert.Equal(2, model.Lessons.Count);
            Assert.Equal(1, model.Lessons.First().OrderNumber); // lessons відсортовані
        }

        [Fact]
        public void ModuleDetails_ReturnsNotFound_ForNonExistingModule()
        {
            var context = GetDbContext();
            var controller = new CourseVerificationController(context);

            var result = controller.ModuleDetails(88);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void ModuleAssignments_ReturnsView_ForExistingModule()
        {
            var context = GetDbContext();
            var controller = new CourseVerificationController(context);

            var result = controller.ModuleAssignments(1);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Module>(viewResult.Model);

            Assert.Equal(1, model.ModuleId);
        }

        [Fact]
        public void ModuleAssignments_ReturnsNotFound_ForNonExistingModule()
        {
            var context = GetDbContext();
            var controller = new CourseVerificationController(context);

            var result = controller.ModuleAssignments(88);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void LessonDetails_ReturnsView_ForExistingLesson()
        {
            var context = GetDbContext();
            var controller = new CourseVerificationController(context);

            var result = controller.LessonDetails(1);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Lesson>(viewResult.Model);

            Assert.Equal(1, model.LessonId);
        }

        [Fact]
        public void LessonDetails_ReturnsNotFound_ForNonExistingLesson()
        {
            var context = GetDbContext();
            var controller = new CourseVerificationController(context);

            var result = controller.LessonDetails(88);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
