using courses_platform.Contexts;
using courses_platform.Models;
using courses_platform.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace courses_platform.Controllers.ApiControllers.V2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.Modules)
                .Include(c => c.Verifications)
                .Include(c => c.Certificates)
                .Select(c => new
                {
                    c.CourseId,
                    c.Title,
                    c.Description,
                    c.CompletedCount,
                })
                .ToListAsync();

            return Ok(new
            {
                version = "2.0",
                generatedAt = DateTime.UtcNow,
                data = courses,
                total = courses.Count
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Modules).ThenInclude(m => m.Lessons)
                .Include(c => c.Verifications)
                .Include(c => c.Certificates)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null) return NotFound();

            return Ok(course);
        }

        [HttpGet("simpleCourses")]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetSimpleCourses()
        {
            var coursesDto = await _context.Courses
                .Select(c => new CourseDto
                {
                    Title = c.Title,
                    Description = c.Description,
                    CompletedCount = c.CompletedCount
                })
                .ToListAsync();

            return Ok(coursesDto);
        }


        [HttpPost]
        public async Task<ActionResult<Course>> PostCourse(CreateCourseDto dto)
        {
            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                CompletedCount = 0,
                Modules = dto.Modules.Select(m => new Module
                {
                    Title = m.Title,
                    ModuleDescription = m.ModuleDescription,
                    OrderNumber = m.OrderNumber,
                    Lessons = m.Lessons.Select(l => new Lesson
                    {
                        Title = l.Title,
                        LessonDescription = l.LessonDescription,
                        OrderNumber = l.OrderNumber
                    }).ToList()
                }).ToList()
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCourse), new { id = course.CourseId }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(int id, Course course)
        {
            if (id != course.CourseId) return BadRequest();

            _context.Entry(course).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Courses.Any(e => e.CourseId == id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
