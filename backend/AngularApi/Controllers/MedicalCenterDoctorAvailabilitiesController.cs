using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AngularApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminPolicy")]
    public class MedicalCenterDoctorAvailabilitiesController : ControllerBase
    {
        private readonly IMedicalCenterDoctorAvailabilityService _availabilityService;

        public MedicalCenterDoctorAvailabilitiesController(IMedicalCenterDoctorAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        // GET: api/MedicalCenterDoctorAvailabilities
        [HttpGet]
        public async Task<ActionResult<PagedResult<MedicalCenterDoctorAvailabilityDTO>>> GetMedicalCenterDoctorAvailability([FromQuery] PaginationParameters pagination)
        {
            return await _availabilityService.GetAllAsync(pagination);
        }

        // GET: api/MedicalCenterDoctorAvailabilities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MedicalCenterDoctorAvailability>> GetMedicalCenterDoctorAvailability(int id)
        {
            var medicalCenterDoctorAvailability = await _availabilityService.GetByIdAsync(id);

            if (medicalCenterDoctorAvailability == null)
            {
                return NotFound();
            }

            return medicalCenterDoctorAvailability;
        }

        // PUT: api/MedicalCenterDoctorAvailabilities/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMedicalCenterDoctorAvailability(int id, MedicalCenterDoctorAvailability medicalCenterDoctorAvailability)
        {
            if (id != medicalCenterDoctorAvailability.Id)
            {
                return BadRequest();
            }

            var updated = await _availabilityService.UpdateAsync(id, medicalCenterDoctorAvailability);
            return updated ? NoContent() : NotFound();
        }

        // POST: api/MedicalCenterDoctorAvailabilities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MedicalCenterDoctorAvailability>> PostMedicalCenterDoctorAvailability(MedicalCenterDoctorAvailability medicalCenterDoctorAvailability)
        {
            var created = await _availabilityService.CreateAsync(medicalCenterDoctorAvailability);
            if (created == null)
            {
                return BadRequest();
            }

            return CreatedAtAction("GetMedicalCenterDoctorAvailability", new { id = created.Id }, created);
        }

        // DELETE: api/MedicalCenterDoctorAvailabilities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedicalCenterDoctorAvailability(int id)
        {
            var deleted = await _availabilityService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
