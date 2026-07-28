using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;
using AngularApi.Contracts.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AngularApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicalCenterDoctorAvailabilitiesController : ControllerBase
    {
        private readonly IMedicalCenterDoctorAvailabilityService _availabilityService;

        public MedicalCenterDoctorAvailabilitiesController(IMedicalCenterDoctorAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        // GET: api/MedicalCenterDoctorAvailabilities
        [Authorize(Roles = "user,doctor,admin")]
        [HttpGet]
        public async Task<ActionResult<PagedResult<MedicalCenterDoctorAvailabilityDTO>>> GetMedicalCenterDoctorAvailability([FromQuery] PaginationParameters pagination)
        {
            return await _availabilityService.GetAllAsync(pagination);
        }

        // GET: api/MedicalCenterDoctorAvailabilities/5
        [Authorize(Roles = "user,doctor,admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<MedicalCenterDoctorAvailabilityDetailDTO>> GetMedicalCenterDoctorAvailability(int id)
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
        [Authorize(Policy = "AdminPolicy")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMedicalCenterDoctorAvailability(int id, UpdateMedicalCenterDoctorAvailabilityDTO dto)
        {
            var updated = await _availabilityService.UpdateAsync(id, dto);
            return updated ? NoContent() : NotFound();
        }

        // POST: api/MedicalCenterDoctorAvailabilities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public async Task<ActionResult<MedicalCenterDoctorAvailability>> PostMedicalCenterDoctorAvailability(CreateMedicalCenterDoctorAvailabilityDTO dto)
        {
            var created = await _availabilityService.CreateAsync(dto);
            if (created == null)
            {
                return BadRequest();
            }

            return CreatedAtAction("GetMedicalCenterDoctorAvailability", new { id = created.Id }, created);
        }

        // DELETE: api/MedicalCenterDoctorAvailabilities/5
        [Authorize(Policy = "AdminPolicy")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedicalCenterDoctorAvailability(int id)
        {
            var deleted = await _availabilityService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
