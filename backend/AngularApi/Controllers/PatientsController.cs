using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AngularApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IOwnershipValidator _ownershipValidator;

        public PatientsController(IPatientService patientService, IOwnershipValidator ownershipValidator)
        {
            _patientService = patientService;
            _ownershipValidator = ownershipValidator;
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpGet]
        public async Task<IActionResult> GetAllPatientsWithReviews([FromQuery] PaginationParameters pagination)
        {
            var patients = await _patientService.GetAllPatientsWithReviewsAsync(pagination);
            return Ok(patients);
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDTO>> GetPatientById(string id)
        {
            if (!_ownershipValidator.CanAccessPatientResource(User, id))
            {
                return Forbid();
            }

            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            return Ok(patient);
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [HttpGet("{patientId}/appointments")]
        public async Task<ActionResult<PagedResult<AppointmentDTO>>> GetPatientAppointments(string patientId, [FromQuery] PaginationParameters pagination)
        {
            if (!_ownershipValidator.CanAccessPatientResource(User, patientId))
            {
                return Forbid();
            }

            return await _patientService.GetPatientAppointmentsAsync(patientId, pagination);
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [HttpGet("{patientId}/appointments/date-range")]
        public async Task<ActionResult<PagedResult<AppointmentDTO>>> GetAppointmentsByDateRange(string patientId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] PaginationParameters pagination)
        {
            if (!_ownershipValidator.CanAccessPatientResource(User, patientId))
            {
                return Forbid();
            }

            return await _patientService.GetAppointmentsByDateRangeAsync(patientId, startDate, endDate, pagination);
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [HttpPut("{patientId}/reviews/{reviewId}")]
        public async Task<IActionResult> UpdateReview(string patientId, int reviewId, [FromBody] PatientReview review)
        {
            if (!_ownershipValidator.CanAccessPatientResource(User, patientId)
                || !string.Equals(review.PatientId, patientId, StringComparison.Ordinal)
                || (!_ownershipValidator.IsAdmin(User)
                    && !string.Equals(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, review.PatientId, StringComparison.Ordinal)))
            {
                return Forbid();
            }

            if (patientId != review.PatientId || reviewId != review.Id)
            {
                return BadRequest("Patient ID or Review ID mismatch.");
            }

            await _patientService.UpdateReviewAsync(review);
            return NoContent();
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [HttpDelete("{patientId}/appointments/{appointmentId}")]
        public async Task<IActionResult> DeleteAppointment(string patientId, int appointmentId)
        {
            if (!_ownershipValidator.CanAccessPatientResource(User, patientId))
            {
                return Forbid();
            }

            var deleted = await _patientService.DeletePatientAppointmentAsync(patientId, appointmentId);
            return deleted ? NoContent() : NotFound();
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(string id, [FromBody] PatientDTO model)
        {
            if (!_ownershipValidator.CanAccessPatientResource(User, id))
            {
                return Forbid();
            }

            var updated = await _patientService.UpdatePatientAsync(id, model);
            return updated ? NoContent() : NotFound();
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(string id)
        {
            var deleted = await _patientService.DeletePatientAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
