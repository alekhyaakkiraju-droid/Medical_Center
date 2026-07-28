using AngularApi.Filters;
﻿using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Enums;
using AngularApi.Contracts.Models;
using AngularApi.Contracts.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AngularApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpGet]
        public async Task<IActionResult> GetAllPatientsWithReviews([FromQuery] PaginationParameters pagination)
        {
            var patients = await _patientService.GetAllPatientsWithReviewsAsync(pagination);
            return Ok(patients);
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [ValidateOwnership(ResourceType.Patient)]
        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDTO>> GetPatientById(string id)
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            return Ok(patient);
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [ValidateOwnership(ResourceType.Patient, "patientId")]
        [HttpGet("{patientId}/appointments")]
        public async Task<ActionResult<PagedResult<AppointmentDTO>>> GetPatientAppointments(string patientId, [FromQuery] PaginationParameters pagination)
        {
            return await _patientService.GetPatientAppointmentsAsync(patientId, pagination);
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [ValidateOwnership(ResourceType.Patient, "patientId")]
        [HttpGet("{patientId}/appointments/date-range")]
        public async Task<ActionResult<PagedResult<AppointmentDTO>>> GetAppointmentsByDateRange(string patientId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] PaginationParameters pagination)
        {
            return await _patientService.GetAppointmentsByDateRangeAsync(patientId, startDate, endDate, pagination);
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [ValidateOwnership(ResourceType.Patient, "patientId")]
        [HttpPut("{patientId}/reviews/{reviewId}")]
        public async Task<IActionResult> UpdateReview(string patientId, int reviewId, [FromBody] PatientReview review)
        {
            if (!string.Equals(review.PatientId, patientId, StringComparison.Ordinal)
                || (!User.IsInRole("admin")
                    && !string.Equals(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, review.PatientId, StringComparison.Ordinal)))
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
        [ValidateOwnership(ResourceType.Patient, "patientId")]
        [HttpDelete("{patientId}/appointments/{appointmentId}")]
        public async Task<IActionResult> DeleteAppointment(string patientId, int appointmentId)
        {
            var deleted = await _patientService.DeletePatientAppointmentAsync(patientId, appointmentId);
            return deleted ? NoContent() : NotFound();
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [ValidateOwnership(ResourceType.Patient)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(string id, [FromBody] PatientDTO model)
        {
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
