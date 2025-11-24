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
    public class LessonContentBlocksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LessonContentBlocksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/LessonContentBlocks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LessonContentBlock>>> GetLessonContentBlocks()
        {
            return await _context.LessonContentBlocks.ToListAsync();
        }

        // GET: api/LessonContentBlocks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LessonContentBlock>> GetLessonContentBlock(int id)
        {
            var lessonContentBlock = await _context.LessonContentBlocks.FindAsync(id);

            if (lessonContentBlock == null)
            {
                return NotFound();
            }

            return lessonContentBlock;
        }

        // PUT: api/LessonContentBlocks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLessonContentBlock(int id, LessonContentBlock lessonContentBlock)
        {
            if (id != lessonContentBlock.LessonContentBlockId)
            {
                return BadRequest();
            }

            _context.Entry(lessonContentBlock).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LessonContentBlockExists(id))
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

        // POST: api/LessonContentBlocks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<LessonContentBlock>> PostLessonContentBlock(LessonContentBlock lessonContentBlock)
        {
            _context.LessonContentBlocks.Add(lessonContentBlock);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLessonContentBlock", new { id = lessonContentBlock.LessonContentBlockId }, lessonContentBlock);
        }

        // DELETE: api/LessonContentBlocks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLessonContentBlock(int id)
        {
            var lessonContentBlock = await _context.LessonContentBlocks.FindAsync(id);
            if (lessonContentBlock == null)
            {
                return NotFound();
            }

            _context.LessonContentBlocks.Remove(lessonContentBlock);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LessonContentBlockExists(int id)
        {
            return _context.LessonContentBlocks.Any(e => e.LessonContentBlockId == id);
        }
    }
}
