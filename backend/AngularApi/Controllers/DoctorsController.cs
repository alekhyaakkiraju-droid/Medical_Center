using AngularApi.Filters;
﻿using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Enums;
using AngularApi.Contracts.Models;
using AngularApi.Contracts.Services.Interfaces;
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

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

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

        [ValidateOwnership(ResourceType.Doctor, "doctorId")]
        [HttpGet("{doctorId}")]
        public async Task<ActionResult<DoctorDetailDTO>> GetDoctor(string doctorId)
        {
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

        [ValidateOwnership(ResourceType.Doctor, "doctorId")]
        [HttpGet("{doctorId}/bookings")]
        public async Task<IActionResult> GetBookings(string doctorId, [FromQuery] PaginationParameters pagination)
        {
            var result = await _doctorService.GetBookingsAsync(doctorId, pagination);
            return Ok(result);
        }

        [ValidateOwnership(ResourceType.Doctor, "doctorId")]
        [HttpGet("{doctorId}/bookings/status/{status}")]
        public async Task<IActionResult> GetBookingsByStatus(string doctorId, AppointmentStatusEnum status, [FromQuery] PaginationParameters pagination)
        {
            var bookings = await _doctorService.GetBookingsByStatusAsync(doctorId, status, pagination);
            return Ok(bookings);
        }

        [ValidateOwnership(ResourceType.Doctor, "doctorId")]
        [HttpGet("{doctorId}/bookings/today")]
        public async Task<IActionResult> GetTodaysBookings(string doctorId, [FromQuery] PaginationParameters pagination)
        {
            var bookings = await _doctorService.GetTodaysBookingsAsync(doctorId, pagination);
            return Ok(bookings);
        }

        [ValidateOwnership(ResourceType.Doctor, "doctorId")]
        [HttpGet("{doctorId}/bookings/UpComing")]
        public async Task<IActionResult> GetUpComingBookings(string doctorId, [FromQuery] PaginationParameters pagination)
        {
            var bookings = await _doctorService.GetUpcomingBookingsAsync(doctorId, pagination);
            return Ok(bookings);
        }

        [ValidateOwnership(ResourceType.Doctor, "doctorId")]
        [HttpGet("{doctorId}/bookings/Last30Days")]
        public async Task<IActionResult> GetLast30DaysBookings(string doctorId, [FromQuery] PaginationParameters pagination)
        {
            var bookings = await _doctorService.GetLast30DaysBookingsAsync(doctorId, pagination);
            return Ok(bookings);
        }

        [ValidateOwnership(ResourceType.Doctor, "doctorId")]
        [HttpGet("{doctorId}/reviews")]
        public async Task<IActionResult> GetReviews(string doctorId, [FromQuery] PaginationParameters pagination)
        {
            var reviews = await _doctorService.GetReviewsAsync(doctorId, pagination);
            return Ok(reviews);
        }

        [ValidateOwnership(ResourceType.Doctor, "doctorId")]
        [HttpGet("{doctorId}/rating")]
        public async Task<IActionResult> GetRating(string doctorId)
        {
            var rating = await _doctorService.GetRatingAsync(doctorId);
            return Ok(rating);
        }

        [ValidateOwnership(ResourceType.Doctor, "doctorId")]
        [HttpGet("{doctorId}/qualifications")]
        public async Task<IActionResult> GetQualifications(string doctorId, [FromQuery] PaginationParameters pagination)
        {
            var qualifications = await _doctorService.GetQualificationsAsync(doctorId, pagination);
            return Ok(qualifications);
        }

        [ValidateOwnership(ResourceType.Doctor, "doctorId")]
        [HttpGet("{doctorId}/specializations")]
        public async Task<IActionResult> GetSpecializations(string doctorId, [FromQuery] PaginationParameters pagination)
        {
            var specializations = await _doctorService.GetSpecializationsAsync(doctorId, pagination);
            return Ok(specializations);
        }

        [ValidateOwnership(ResourceType.Doctor, "doctorId")]
        [HttpPut("{doctorId}")]
        public async Task<IActionResult> PutDoctor(string id, Doctor doctor)
        {
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

        [ValidateOwnership(ResourceType.Doctor, "doctorId")]
        [Authorize(Policy = "AdminPolicy")]
        [HttpDelete("{doctorId}")]
        public async Task<IActionResult> DeleteDoctor(string id)
        {
            var deleted = await _doctorService.DeleteDoctorAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [ValidateOwnership(ResourceType.Doctor, "doctorId")]
        [HttpDelete("{doctorId}/appointments/{appointmentId}")]
        public async Task<IActionResult> DeleteAppointment(string doctorId, int appointmentId)
        {
            var canceled = await _doctorService.CancelDoctorAppointmentAsync(doctorId, appointmentId);
            return canceled
                ? NoContent()
                : NotFound(new { message = "Appointment not found" });
        }
    }
}
