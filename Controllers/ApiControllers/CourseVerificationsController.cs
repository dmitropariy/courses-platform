using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using courses_platform.Contexts;
using courses_platform.Models;

namespace courses_platform.Controllers.ApiControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CourseVerificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CourseVerificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CourseVerifications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseVerification>>> GetCourseVerifications()
        {
            return await _context.CourseVerifications.ToListAsync();
        }

        // GET: api/CourseVerifications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseVerification>> GetCourseVerification(int id)
        {
            var courseVerification = await _context.CourseVerifications.FindAsync(id);

            if (courseVerification == null)
            {
                return NotFound();
            }

            return courseVerification;
        }

        // PUT: api/CourseVerifications/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourseVerification(int id, CourseVerification courseVerification)
        {
            if (id != courseVerification.VerificationId)
            {
                return BadRequest();
            }

            _context.Entry(courseVerification).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseVerificationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CourseVerifications
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CourseVerification>> PostCourseVerification(CourseVerification courseVerification)
        {
            _context.CourseVerifications.Add(courseVerification);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCourseVerification", new { id = courseVerification.VerificationId }, courseVerification);
        }

        // DELETE: api/CourseVerifications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseVerification(int id)
        {
            var courseVerification = await _context.CourseVerifications.FindAsync(id);
            if (courseVerification == null)
            {
                return NotFound();
            }

            _context.CourseVerifications.Remove(courseVerification);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CourseVerificationExists(int id)
        {
            return _context.CourseVerifications.Any(e => e.VerificationId == id);
        }
    }
}
