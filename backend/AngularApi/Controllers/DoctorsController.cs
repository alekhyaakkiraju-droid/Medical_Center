using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AngularApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "DoctorOrAdminPolicy")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly IOwnershipValidator _ownershipValidator;

        public DoctorsController(IDoctorService doctorService, IOwnershipValidator ownershipValidator)
        {
            _doctorService = doctorService;
            _ownershipValidator = ownershipValidator;
        }

        private bool IsDoctorAccessDenied(string doctorId) =>
            !_ownershipValidator.CanAccessDoctorResource(User, doctorId);

        [HttpGet]
        public async Task<ActionResult<PagedResult<DoctorDTO>>> GetDoctors([FromQuery] PaginationParameters pagination)
        {
            return await _doctorService.GetDoctorsAsync(pagination);
        }

        [AllowAnonymous]
        [HttpGet("/api/DoctorsWithSpectialization")]
        public async Task<ActionResult<PagedResult<DoctorDTO>>> GetDoctorsWithSpectialization([FromQuery] PaginationParameters pagination)
        {
            return await _doctorService.GetDoctorsWithSpecializationAsync(pagination);
        }

        [HttpGet("{doctorId}")]
        public async Task<ActionResult<Doctor>> GetDoctor(string doctorId)
        {
            if (IsDoctorAccessDenied(doctorId))
            {
                return Forbid();
            }

            var doctor = await _doctorService.GetDoctorByIdAsync(doctorId);
            return doctor != null ? Ok(doctor) : NotFound();
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public async Task<ActionResult<Doctor>> PostDoctor(Doctor doctor)
        {
            var createdDoctor = await _doctorService.CreateDoctorAsync(doctor);
            return CreatedAtAction("GetDoctor", new { id = createdDoctor.Id }, createdDoctor);
        }

        [HttpGet("{doctorId}/bookings")]
        public async Task<IActionResult> GetBookings(string doctorId, [FromQuery] PaginationParameters pagination)
        {
            if (IsDoctorAccessDenied(doctorId))
            {
                return Forbid();
            }

            var result = await _doctorService.GetBookingsAsync(doctorId, pagination);
            return Ok(result);
        }

        [HttpGet("{doctorId}/bookings/status/{status}")]
        public async Task<IActionResult> GetBookingsByStatus(string doctorId, AppointmentStatusEnum status, [FromQuery] PaginationParameters pagination)
        {
            if (IsDoctorAccessDenied(doctorId))
            {
                return Forbid();
            }

            var bookings = await _doctorService.GetBookingsByStatusAsync(doctorId, status, pagination);
            return Ok(bookings);
        }

        [HttpGet("{doctorId}/bookings/today")]
        public async Task<IActionResult> GetTodaysBookings(string doctorId, [FromQuery] PaginationParameters pagination)
        {
            if (IsDoctorAccessDenied(doctorId))
            {
                return Forbid();
            }

            var bookings = await _doctorService.GetTodaysBookingsAsync(doctorId, pagination);
            return Ok(bookings);
        }

        [HttpGet("{doctorId}/bookings/UpComing")]
        public async Task<IActionResult> GetUpComingBookings(string doctorId, [FromQuery] PaginationParameters pagination)
        {
            if (IsDoctorAccessDenied(doctorId))
            {
                return Forbid();
            }

            var bookings = await _doctorService.GetUpcomingBookingsAsync(doctorId, pagination);
            return Ok(bookings);
        }

        [HttpGet("{doctorId}/bookings/Last30Days")]
        public async Task<IActionResult> GetLast30DaysBookings(string doctorId, [FromQuery] PaginationParameters pagination)
        {
            if (IsDoctorAccessDenied(doctorId))
            {
                return Forbid();
            }

            var bookings = await _doctorService.GetLast30DaysBookingsAsync(doctorId, pagination);
            return Ok(bookings);
        }

        [HttpGet("{doctorId}/reviews")]
        public async Task<IActionResult> GetReviews(string doctorId, [FromQuery] PaginationParameters pagination)
        {
            if (IsDoctorAccessDenied(doctorId))
            {
                return Forbid();
            }

            var reviews = await _doctorService.GetReviewsAsync(doctorId, pagination);
            return Ok(reviews);
        }

        [HttpGet("{doctorId}/rating")]
        public async Task<IActionResult> GetRating(string doctorId)
        {
            if (IsDoctorAccessDenied(doctorId))
            {
                return Forbid();
            }

            var rating = await _doctorService.GetRatingAsync(doctorId);
            return Ok(rating);
        }

        [HttpGet("{doctorId}/qualifications")]
        public async Task<IActionResult> GetQualifications(string doctorId, [FromQuery] PaginationParameters pagination)
        {
            if (IsDoctorAccessDenied(doctorId))
            {
                return Forbid();
            }

            var qualifications = await _doctorService.GetQualificationsAsync(doctorId, pagination);
            return Ok(qualifications);
        }

        [HttpGet("{doctorId}/specializations")]
        public async Task<IActionResult> GetSpecializations(string doctorId, [FromQuery] PaginationParameters pagination)
        {
            if (IsDoctorAccessDenied(doctorId))
            {
                return Forbid();
            }

            var specializations = await _doctorService.GetSpecializationsAsync(doctorId, pagination);
            return Ok(specializations);
        }

        [HttpPut("{doctorId}")]
        public async Task<IActionResult> PutDoctor(string id, Doctor doctor)
        {
            if (IsDoctorAccessDenied(id))
            {
                return Forbid();
            }

            if (id != doctor.Id)
            {
                return BadRequest();
            }

            var updated = await _doctorService.UpdateDoctorAsync(id, doctor);
            return updated ? NoContent() : NotFound();
        }

        [HttpPut("bookings/{bookingId}")]
        public async Task<IActionResult> UpdateBooking(int bookingId, [FromBody] Appointment updatedBooking)
        {
            var updated = await _doctorService.UpdateBookingAsync(bookingId, updatedBooking);
            return updated ? NoContent() : NotFound();
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpDelete("{doctorId}")]
        public async Task<IActionResult> DeleteDoctor(string id)
        {
            var deleted = await _doctorService.DeleteDoctorAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpDelete("{doctorId}/appointments/{appointmentId}")]
        public async Task<IActionResult> DeleteAppointment(string doctorId, int appointmentId)
        {
            if (IsDoctorAccessDenied(doctorId))
            {
                return Forbid();
            }

            var canceled = await _doctorService.CancelDoctorAppointmentAsync(doctorId, appointmentId);
            return canceled
                ? NoContent()
                : NotFound(new { message = "Appointment not found" });
        }
    }
}
