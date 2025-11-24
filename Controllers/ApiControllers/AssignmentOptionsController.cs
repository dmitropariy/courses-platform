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
    public class AssignmentOptionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AssignmentOptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/AssignmentOptions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssignmentOption>>> GetAssignmentOptions()
        {
            return await _context.AssignmentOptions.ToListAsync();
        }

        // GET: api/AssignmentOptions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AssignmentOption>> GetAssignmentOption(int id)
        {
            var assignmentOption = await _context.AssignmentOptions.FindAsync(id);

            if (assignmentOption == null)
            {
                return NotFound();
            }

            return assignmentOption;
        }

        // PUT: api/AssignmentOptions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssignmentOption(int id, AssignmentOption assignmentOption)
        {
            if (id != assignmentOption.OptionId)
            {
                return BadRequest();
            }

            _context.Entry(assignmentOption).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssignmentOptionExists(id))
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

        // POST: api/AssignmentOptions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AssignmentOption>> PostAssignmentOption(AssignmentOption assignmentOption)
        {
            _context.AssignmentOptions.Add(assignmentOption);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAssignmentOption", new { id = assignmentOption.OptionId }, assignmentOption);
        }

        // DELETE: api/AssignmentOptions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssignmentOption(int id)
        {
            var assignmentOption = await _context.AssignmentOptions.FindAsync(id);
            if (assignmentOption == null)
            {
                return NotFound();
            }

            _context.AssignmentOptions.Remove(assignmentOption);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AssignmentOptionExists(int id)
        {
            return _context.AssignmentOptions.Any(e => e.OptionId == id);
        }
    }
}
