using Microsoft.EntityFrameworkCore;
using courses_platform.Controllers;
using courses_platform.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace courses_platform.Tests.Controllers
{
    public class CoursesControllerTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            var course1 = new Course { CourseId = 1, Title = "C# Fundamentals", Description = "Desc1" };
            var course2 = new Course { CourseId = 2, Title = "Python Advanced", Description = "Desc2"  };
            var course3 = new Course { CourseId = 3, Title = "Java Basics", Description = "Desc3"  };

            context.Courses.AddRange(course1, course2, course3);

            context.CourseVerifications.AddRange(
                new CourseVerification { CourseId = 1, Status = "approved" },
                new CourseVerification { CourseId = 2, Status = "rejected" },
                new CourseVerification { CourseId = 3, Status = "approved" }
            );

            context.SaveChanges();
            return context;
        }

        [Fact]
        public void Index_ReturnsOnlyApprovedCourses()
        {
            var context = GetDbContext();
            var controller = new CoursesController(context);

            var result = controller.Index(null, 1) as ViewResult;
            var model = result?.Model as List<Course>;

            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal(2, model.Count); // only 2 approved
            Assert.All(model, c =>
                Assert.Contains(c.Verifications, v => v.Status == "approved"));
        }

        [Fact]
        public void Index_SearchFiltersCourses()
        {
            var context = GetDbContext();
            var controller = new CoursesController(context);

            var result = controller.Index("java", 1) as ViewResult;
            var model = result?.Model as List<Course>;

            Assert.Single(model);
            Assert.Equal("Java Basics", model[0].Title);
        }
    }
}
