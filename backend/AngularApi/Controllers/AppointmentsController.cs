using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services;
using AngularApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public AppointmentsController(
            MedicalCenterDbContext context,
            UserManager<AppUser> userManager,
            IEmailService emailService,
            EmailTemplateService emailTemplateService,
            IOwnershipValidator ownershipValidator)
        {
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
            _ownershipValidator = ownershipValidator;
        }

        //public AppointmentsController(MedicalCenterDbContext context, UserManager<AppUser> userManager, IEmailService emailService)
        //{
        //    _userManager = userManager;
        //    _context = context;
        //    _emailService = emailService;
        //}


        [Authorize(Policy = "AdminPolicy")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments()
        {
            return await _context.Appointments.Include(i => i.Patient).ToListAsync();
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpGet("GetAllAppointments")]
        public async Task<ActionResult<IEnumerable<AppointmentDTO>>> GetAllAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.DoctorSpecializations)!
                        .ThenInclude(ds => ds.Specialization)
                .Include(a => a.Patient)
                .ToListAsync();

            var appointmentDtos = appointments.Select(appointment => new AppointmentDTO
            {
                AppointmentId = appointment.Id,
                AppointmentDate = appointment.AppointmentTakenDate,
                Doctor = new DoctorDTO
                {
                    Name = appointment.DoctorName,
                    Specializations = appointment.Doctor?.DoctorSpecializations
                        .Select(ds => ds.Specialization?.SpecializationName)
                        .ToList() ?? new List<string>()
                },
                Patient = new PatientDTO
                {
                    PatientId = appointment.PatientId.ToString(),
                    Name = appointment.Patient?.UserName,
                    Email = appointment.Patient?.Email,
                }
            }).ToList();

            return Ok(appointmentDtos);
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

            if (!string.IsNullOrEmpty(appointment.DoctorName))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Name == appointment.DoctorName);
                if (doctor != null)
                {
                    appointment.DoctorId = doctor.Id;
                }
                else
                {
                    return BadRequest("Invalid DoctorName");
                }
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            appointment.PatientId = user.Id;
            appointment.MedicalCenterId = 2;
            appointment.AppointmentStatusId = (int)AppointmentStatusEnum.Active + (int)1;
            appointment.Amount = 30;
            appointment.PaymentStatus = "Pending";

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var emailTemplateService = HttpContext.RequestServices.GetRequiredService<EmailTemplateService>();

            var emailBody = _emailTemplateService.GetAppointmentConfirmationEamil(user.UserName, appointment.DoctorName, appointment.AppointmentTakenDate.ToString());
            var messageObj = new Message(new[] { user.Email }, "Appointment Confirmation", emailBody);

            _emailService.SendEmail(messageObj);
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
        public async Task<IActionResult> GetAppointmentsByPatient(string patientId)
        {
            if (!_ownershipValidator.CanAccessPatientResource(User, patientId))
            {
                return Forbid();
            }

            var appointments = await _context.Appointments
                .Include(a => a.AppointmentStatus)
                .Where(a => a.PatientId == patientId)
                .Select(a => new
                {
                    a.Id,
                    a.PatientId,
                    a.DoctorName,
                    a.ProbableStartTime,
                    a.ActualEndTime,
                    AppointmentStatus = a.AppointmentStatus != null ? a.AppointmentStatus.Status.ToString() : null
                })
                .ToListAsync();

            return Ok(appointments);
        }


        [Authorize(Policy = "DoctorPolicy")]
        [HttpGet("date/{date}")]
        public async Task<IActionResult> GetAppointmentsByDate(DateTime date)
        {
            var appointments = await _context.Appointments
                .Where(a => a.ProbableStartTime.Value.Date == date.Date)
                .ToListAsync();
            return Ok(appointments);
        }

        [Authorize(Policy = "DoctorPolicy")]
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetAppointmentsByStatus(AppointmentStatusEnum status)
        {
            var appointments = await _context.Appointments
                .Where(a => a.AppointmentStatus!.Status == status)
                .ToListAsync();
            return Ok(appointments);
        }

        [Authorize(Policy = "DoctorPolicy")]
        [HttpGet("today")]
        public async Task<IActionResult> GetTodaysAppointments()
        {
            var today = DateTime.Today;
            var appointments = await _context.Appointments
                .Where(a => a.ProbableStartTime!.Value.Date == today)
                .ToListAsync();
            return Ok(appointments);
        }


        [Authorize(Policy = "DoctorPolicy")]
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingAppointments()
        {
            var now = DateTime.Now;
            var appointments = await _context.Appointments
                .Where(a => a.ProbableStartTime > now)
                .OrderBy(a => a.ProbableStartTime)
                .ToListAsync();
            return Ok(appointments);
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [HttpGet("patient/{patientId}/status/{status}")]
        public async Task<IActionResult> GetAppointmentsByPatientAndStatus(string patientId, AppointmentStatusEnum status)
        {
            if (!_ownershipValidator.CanAccessPatientResource(User, patientId))
            {
                return Forbid();
            }

            var appointments = await _context.Appointments
                .Where(a => a.PatientId == patientId && a.AppointmentStatus!.Status == status)
                .ToListAsync();
            return Ok(appointments);
        }

        [Authorize(Policy = "UserOrAdminPolicy")]
        [HttpGet("patient/{patientId}/history")]
        public async Task<IActionResult> GetAppointmentHistoryByPatient(string patientId)
        {
            if (!_ownershipValidator.CanAccessPatientResource(User, patientId))
            {
                return Forbid();
            }

            var appointments = await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.ProbableStartTime)
                .ToListAsync();
            return Ok(appointments);
        }


        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
