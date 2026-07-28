using AngularApi.Filters;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Enums;
using AngularApi.Contracts.Models;
using AngularApi.Contracts.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AngularApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly UserManager<AppUser> _userManager;

        public AppointmentsController(
            IAppointmentService appointmentService,
            UserManager<AppUser> userManager)
        {
            _appointmentService = appointmentService;
            _userManager = userManager;
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpGet]
        public async Task<ActionResult<PagedResult<AppointmentDTO>>> GetAppointments([FromQuery] PaginationParameters pagination)
        {
            return await _appointmentService.GetAppointmentsAsync(pagination);
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpGet("GetAllAppointments")]
        public async Task<ActionResult<PagedResult<AppointmentDTO>>> GetAllAppointments([FromQuery] PaginationParameters pagination)
        {
            return await _appointmentService.GetAllAppointmentsAsync(pagination);
        }

        [Authorize(Roles = "user,doctor,admin")]
        [ValidateOwnership(ResourceType.Appointment)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Appointment>> GetAppointment(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            return appointment;
        }

        [Authorize(Roles = "user,doctor,admin")]
        [ValidateOwnership(ResourceType.Appointment)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentDTO appointmentDto)
        {
            if (id != appointmentDto.Id)
            {
                return BadRequest("Appointment ID mismatch.");
            }

            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var updated = await _appointmentService.UpdateAppointmentAsync(id, appointmentDto);
            if (!updated)
            {
                return StatusCode(500, "An error occurred while updating the appointment.");
            }

            return NoContent();
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpGet("total-earnings")]
        public async Task<IActionResult> GetPatientTotalEarnings()
        {
            var totalEarnings = await _appointmentService.GetTotalEarningsAsync();
            return Ok(new { TotalEarnings = totalEarnings });
        }

        [Authorize(Policy = "UserPolicy")]
        [HttpPost]
        public async Task<ActionResult<AppointmentDTO>> PostAppointment([FromBody] CreateAppointmentDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var (createdAppointment, errorMessage) = await _appointmentService.CreateAppointmentAsync(dto, userId!);
            if (errorMessage != null)
            {
                return BadRequest(errorMessage);
            }

            return CreatedAtAction(nameof(GetAppointment), new { id = createdAppointment!.AppointmentId }, createdAppointment);
        }

        [Authorize(Roles = "admin,doctor")]
        [ValidateOwnership(ResourceType.Appointment)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var deleted = await _appointmentService.DeleteAppointmentAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [ValidateOwnership(ResourceType.Patient, "patientId")]
        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetAppointmentsByPatient(string patientId, [FromQuery] PaginationParameters pagination)
        {
            var appointments = await _appointmentService.GetAppointmentsByPatientAsync(patientId, pagination);
            return Ok(appointments);
        }

        [Authorize(Policy = "DoctorPolicy")]
        [HttpGet("date/{date}")]
        public async Task<IActionResult> GetAppointmentsByDate(DateTime date, [FromQuery] PaginationParameters pagination)
        {
            var appointments = await _appointmentService.GetAppointmentsByDateAsync(date, pagination);
            return Ok(appointments);
        }

        [Authorize(Policy = "DoctorPolicy")]
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetAppointmentsByStatus(AppointmentStatusEnum status, [FromQuery] PaginationParameters pagination)
        {
            var appointments = await _appointmentService.GetAppointmentsByStatusAsync(status, pagination);
            return Ok(appointments);
        }

        [Authorize(Policy = "DoctorPolicy")]
        [HttpGet("today")]
        public async Task<IActionResult> GetTodaysAppointments([FromQuery] PaginationParameters pagination)
        {
            var appointments = await _appointmentService.GetTodaysAppointmentsAsync(pagination);
            return Ok(appointments);
        }

        [Authorize(Policy = "DoctorPolicy")]
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingAppointments([FromQuery] PaginationParameters pagination)
        {
            var appointments = await _appointmentService.GetUpcomingAppointmentsAsync(pagination);
            return Ok(appointments);
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [ValidateOwnership(ResourceType.Patient, "patientId")]
        [HttpGet("patient/{patientId}/status/{status}")]
        public async Task<IActionResult> GetAppointmentsByPatientAndStatus(string patientId, AppointmentStatusEnum status, [FromQuery] PaginationParameters pagination)
        {
            var appointments = await _appointmentService.GetAppointmentsByPatientAndStatusAsync(patientId, status, pagination);
            return Ok(appointments);
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [ValidateOwnership(ResourceType.Patient, "patientId")]
        [HttpGet("patient/{patientId}/history")]
        public async Task<IActionResult> GetAppointmentHistoryByPatient(string patientId, [FromQuery] PaginationParameters pagination)
        {
            var appointments = await _appointmentService.GetAppointmentHistoryByPatientAsync(patientId, pagination);
            return Ok(appointments);
        }
    }
}
