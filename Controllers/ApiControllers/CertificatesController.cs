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
    public class CertificatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CertificatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CertificatesControllerApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Certificate>>> GetCertificates()
        {
            return await _context.Certificates.ToListAsync();
        }

        // GET: api/CertificatesControllerApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Certificate>> GetCertificate(int id)
        {
            var certificate = await _context.Certificates.FindAsync(id);

            if (certificate == null)
            {
                return NotFound();
            }

            return certificate;
        }

        // PUT: api/CertificatesControllerApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCertificate(int id, Certificate certificate)
        {
            if (id != certificate.CertificateId)
            {
                return BadRequest();
            }

            _context.Entry(certificate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CertificateExists(id))
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

        // POST: api/CertificatesControllerApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Certificate>> PostCertificate(Certificate certificate)
        {
            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCertificate", new { id = certificate.CertificateId }, certificate);
        }

        // DELETE: api/CertificatesControllerApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCertificate(int id)
        {
            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate == null)
            {
                return NotFound();
            }

            _context.Certificates.Remove(certificate);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CertificateExists(int id)
        {
            return _context.Certificates.Any(e => e.CertificateId == id);
        }
    }
}
