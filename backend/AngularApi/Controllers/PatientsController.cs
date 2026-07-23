using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientsController : ControllerBase
    {

        private readonly MedicalCenterDbContext _context;
        private readonly IOwnershipValidator _ownershipValidator;

        public PatientsController(MedicalCenterDbContext context, IOwnershipValidator ownershipValidator)
        {
            _context = context;
            _ownershipValidator = ownershipValidator;
        }
        [Authorize(Policy = "AdminPolicy")]
        [HttpGet]
        public async Task<IActionResult> GetAllPatientsWithReviews([FromQuery] PaginationParameters pagination)
        {
            var patients = await _context.Patients
                .Select(p => new PatientDTO
                {
                    PatientId = p.Id,
                    Name = p.Name,
                    Email = p.Email,
                    Image = p.Image,
                    Reviews = p.PatientReview.Select(r => new ReviewDTO
                    {
                        Id = r.Id,
                        PatientId = r.PatientId,
                        DoctorId = r.DoctorId,
                        IsReviewAnonymous = r.IsReviewAnonymous,
                        WaitTimeRating = r.WaitTimeRating,
                        BedsideMannerRating = r.BedsideMannerRating,
                        OverallRating = r.OverallRating,
                        Review = r.Review,
                        IsDoctorRecommended = r.IsDoctorRecommended,
                        ReviewDate = r.ReviewDate
                    }).ToList()
                })
                .ToPagedResultAsync(pagination);

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

            var patient = await _context.Patients
                .Where(p => p.Id == id)
                .Select(p => new PatientDTO
                {
                    PatientId = p.Id,
                    Name = p.Name,
                    Email = p.Email,
                    Image = p.Image
                })
                .FirstOrDefaultAsync();

            if (patient == null) return NotFound();

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

            return await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .SelectAppointmentDto()
                .ToPagedResultAsync(pagination);
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [HttpGet("{patientId}/appointments/date-range")]
        public async Task<ActionResult<PagedResult<AppointmentDTO>>> GetAppointmentsByDateRange(string patientId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] PaginationParameters pagination)
        {
            if (!_ownershipValidator.CanAccessPatientResource(User, patientId))
            {
                return Forbid();
            }

            return await _context.Appointments
                .Where(a => a.PatientId == patientId && a.AppointmentTakenDate >= startDate && a.AppointmentTakenDate <= endDate)
                .SelectAppointmentDto()
                .ToPagedResultAsync(pagination);
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

            _context.Entry(review).State = EntityState.Modified;
            await _context.SaveChangesAsync();

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

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patientId);
            if (appointment == null) return NotFound();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(string id, [FromBody] PatientDTO model)
        {
            if (!_ownershipValidator.CanAccessPatientResource(User, id))
            {
                return Forbid();
            }

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            patient.Name = model.Name;
            patient.Email = model.Email;
            patient.Image = model.Image;

            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [Authorize(Policy = "AdminPolicy")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(string id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
