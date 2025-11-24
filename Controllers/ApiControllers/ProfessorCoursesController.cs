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
    public class ProfessorCoursesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProfessorCoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ProfessorCourses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProfessorCourse>>> GetProfessorCourses()
        {
            return await _context.ProfessorCourses.ToListAsync();
        }

        // GET: api/ProfessorCourses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProfessorCourse>> GetProfessorCourse(int id)
        {
            var professorCourse = await _context.ProfessorCourses.FindAsync(id);

            if (professorCourse == null)
            {
                return NotFound();
            }

            return professorCourse;
        }

        // PUT: api/ProfessorCourses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProfessorCourse(int id, ProfessorCourse professorCourse)
        {
            if (id != professorCourse.Id)
            {
                return BadRequest();
            }

            _context.Entry(professorCourse).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfessorCourseExists(id))
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

        // POST: api/ProfessorCourses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProfessorCourse>> PostProfessorCourse(ProfessorCourse professorCourse)
        {
            _context.ProfessorCourses.Add(professorCourse);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProfessorCourse", new { id = professorCourse.Id }, professorCourse);
        }

        // DELETE: api/ProfessorCourses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfessorCourse(int id)
        {
            var professorCourse = await _context.ProfessorCourses.FindAsync(id);
            if (professorCourse == null)
            {
                return NotFound();
            }

            _context.ProfessorCourses.Remove(professorCourse);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProfessorCourseExists(int id)
        {
            return _context.ProfessorCourses.Any(e => e.Id == id);
        }
    }
}
