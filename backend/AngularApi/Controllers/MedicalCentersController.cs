using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngularApi.DTO;
using AngularApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminPolicy")]
    public class MedicalCentersController : ControllerBase
    {
        private readonly MedicalCenterDbContext _context;

        public MedicalCentersController(MedicalCenterDbContext context)
        {
            _context = context;
        }

        // GET: api/MedicalCenters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicalCenterListItemDTO>>> GetMedicalCenter()
        {
            return await _context.MedicalCenter
                .Select(m => new MedicalCenterListItemDTO
                {
                    Id = m.Id,
                    HospitalAffiliationId = m.HospitalAffiliationId,
                    TimeSlotPerClientInMin = m.TimeSlotPerClientInMin,
                    FirstConsultationFee = m.FirstConsultationFee,
                    FollowupConsultationFee = m.FollowupConsultationFee,
                    StreetAddress = m.StreetAddress,
                    City = m.City,
                    State = m.State,
                    Zip = m.Zip
                })
                .ToListAsync();
        }

        // GET: api/MedicalCenters/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MedicalCenter>> GetMedicalCenter(int id)
        {
            var medicalCenter = await _context.MedicalCenter.FindAsync(id);

            if (medicalCenter == null)
            {
                return NotFound();
            }

            return medicalCenter;
        }

        // PUT: api/MedicalCenters/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMedicalCenter(int id, MedicalCenter medicalCenter)
        {
            if (id != medicalCenter.Id)
            {
                return BadRequest();
            }

            _context.Entry(medicalCenter).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MedicalCenterExists(id))
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

        // POST: api/MedicalCenters
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MedicalCenter>> PostMedicalCenter(MedicalCenter medicalCenter)
        {
            _context.MedicalCenter.Add(medicalCenter);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMedicalCenter", new { id = medicalCenter.Id }, medicalCenter);
        }

        // DELETE: api/MedicalCenters/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedicalCenter(int id)
        {
            var medicalCenter = await _context.MedicalCenter.FindAsync(id);
            if (medicalCenter == null)
            {
                return NotFound();
            }

            _context.MedicalCenter.Remove(medicalCenter);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MedicalCenterExists(int id)
        {
            return _context.MedicalCenter.Any(e => e.Id == id);
        }
    }
}
