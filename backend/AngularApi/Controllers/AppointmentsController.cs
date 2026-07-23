using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Options;
using AngularApi.Services;
using AngularApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AngularApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly MedicalCenterDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly EmailTemplateService _emailTemplateService;
        private readonly IEmailService _emailService;
        private readonly IOwnershipValidator _ownershipValidator;
        private readonly AppointmentSettings _appointmentSettings;

        public AppointmentsController(
            MedicalCenterDbContext context,
            UserManager<AppUser> userManager,
            IEmailService emailService,
            EmailTemplateService emailTemplateService,
            IOwnershipValidator ownershipValidator,
            IOptions<AppointmentSettings> appointmentSettings)
        {
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
            _ownershipValidator = ownershipValidator;
            _appointmentSettings = appointmentSettings.Value;
        }

        //public AppointmentsController(MedicalCenterDbContext context, UserManager<AppUser> userManager, IEmailService emailService)
        //{
        //    _userManager = userManager;
        //    _context = context;
        //    _emailService = emailService;
        //}


        [Authorize(Policy = "AdminPolicy")]
        [HttpGet]
        public async Task<ActionResult<PagedResult<AppointmentDTO>>> GetAppointments([FromQuery] PaginationParameters pagination)
        {
            return await _context.Appointments.SelectAppointmentDto().ToPagedResultAsync(pagination);
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpGet("GetAllAppointments")]
        public async Task<ActionResult<PagedResult<AppointmentDTO>>> GetAllAppointments([FromQuery] PaginationParameters pagination)
        {
            return await _context.Appointments.SelectAppointmentDto().ToPagedResultAsync(pagination);
        }



        [Authorize(Policy = "UserPolicy")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Appointment>> GetAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            return appointment;
        }


        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutAppointment(int id, Appointment appointment)
        //{
        //    if (id != appointment.Id)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(appointment).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!AppointmentExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        [Authorize(Policy = "UserPolicy")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentDTO appointmentDto)
        {
            if (id != appointmentDto.Id)
            {
                return BadRequest("Appointment ID mismatch.");
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            appointment.AppointmentTakenDate = appointmentDto.AppointmentTakenDate;

            _context.Entry(appointment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "An error occurred while updating the appointment.");
            }

            return NoContent();
        }


        [Authorize(Policy = "AdminPolicy")]
        [HttpGet("total-earnings")]
        public async Task<IActionResult> GetPatientTotalEarnings()
        {
            var totalEarnings = await _context.Appointments
                .SumAsync(p => p.Amount);

            return Ok(new { TotalEarnings = totalEarnings });
        }

        [Authorize(Policy = "UserPolicy")]
        [HttpPost]
        public async Task<ActionResult<Appointment>> PostAppointment(Appointment appointment)
        {
            if (string.IsNullOrEmpty(appointment.DoctorId))
            {
                return BadRequest("DoctorId is required");
            }

            var doctor = await _context.Doctors.FindAsync(appointment.DoctorId);
            if (doctor == null)
            {
                return BadRequest("Invalid DoctorId");
            }

            appointment.DoctorName = doctor.Name;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            appointment.PatientId = user.Id;
            appointment.MedicalCenterId = _appointmentSettings.DefaultCenterId;
            appointment.AppointmentStatusId = (int)AppointmentStatusEnum.Active;
            appointment.Amount = _appointmentSettings.DefaultFee;
            appointment.PaymentStatus = "Pending";

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            try
            {
                var emailBody = _emailTemplateService.GetAppointmentConfirmationEmail(
                    user.UserName,
                    appointment.DoctorName,
                    appointment.AppointmentTakenDate.ToString());
                var messageObj = new Message(new[] { user.Email }, "Appointment Confirmation", emailBody);
                await _emailService.SendEmailAsync(messageObj);
            }
            catch (Exception)
            {
                // Email failure must not prevent appointment creation.
            }

            return CreatedAtAction("GetAppointment", new { id = appointment.Id }, appointment);
        }

        [Authorize(Roles = "admin,doctor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetAppointmentsByPatient(string patientId, [FromQuery] PaginationParameters pagination)
        {
            if (!_ownershipValidator.CanAccessPatientResource(User, patientId))
            {
                return Forbid();
            }

            var appointments = await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .SelectAppointmentDto()
                .ToPagedResultAsync(pagination);

            return Ok(appointments);
        }


        [Authorize(Policy = "DoctorPolicy")]
        [HttpGet("date/{date}")]
        public async Task<IActionResult> GetAppointmentsByDate(DateTime date, [FromQuery] PaginationParameters pagination)
        {
            var appointments = await _context.Appointments
                .Where(a => a.ProbableStartTime!.Value.Date == date.Date)
                .SelectAppointmentDto()
                .ToPagedResultAsync(pagination);
            return Ok(appointments);
        }

        [Authorize(Policy = "DoctorPolicy")]
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetAppointmentsByStatus(AppointmentStatusEnum status, [FromQuery] PaginationParameters pagination)
        {
            var appointments = await _context.Appointments
                .Where(a => a.AppointmentStatus!.Status == status)
                .SelectAppointmentDto()
                .ToPagedResultAsync(pagination);
            return Ok(appointments);
        }

        [Authorize(Policy = "DoctorPolicy")]
        [HttpGet("today")]
        public async Task<IActionResult> GetTodaysAppointments([FromQuery] PaginationParameters pagination)
        {
            var today = DateTime.Today;
            var appointments = await _context.Appointments
                .Where(a => a.ProbableStartTime!.Value.Date == today)
                .SelectAppointmentDto()
                .ToPagedResultAsync(pagination);
            return Ok(appointments);
        }


        [Authorize(Policy = "DoctorPolicy")]
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingAppointments([FromQuery] PaginationParameters pagination)
        {
            var now = DateTime.Now;
            var appointments = await _context.Appointments
                .Where(a => a.ProbableStartTime > now)
                .OrderBy(a => a.ProbableStartTime)
                .SelectAppointmentDto()
                .ToPagedResultAsync(pagination);
            return Ok(appointments);
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [HttpGet("patient/{patientId}/status/{status}")]
        public async Task<IActionResult> GetAppointmentsByPatientAndStatus(string patientId, AppointmentStatusEnum status, [FromQuery] PaginationParameters pagination)
        {
            if (!_ownershipValidator.CanAccessPatientResource(User, patientId))
            {
                return Forbid();
            }

            var appointments = await _context.Appointments
                .Where(a => a.PatientId == patientId && a.AppointmentStatus!.Status == status)
                .SelectAppointmentDto()
                .ToPagedResultAsync(pagination);
            return Ok(appointments);
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [HttpGet("patient/{patientId}/history")]
        public async Task<IActionResult> GetAppointmentHistoryByPatient(string patientId, [FromQuery] PaginationParameters pagination)
        {
            if (!_ownershipValidator.CanAccessPatientResource(User, patientId))
            {
                return Forbid();
            }

            var appointments = await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.ProbableStartTime)
                .SelectAppointmentDto()
                .ToPagedResultAsync(pagination);
            return Ok(appointments);
        }


        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
