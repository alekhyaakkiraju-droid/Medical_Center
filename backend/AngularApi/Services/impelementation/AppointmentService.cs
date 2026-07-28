using System.Net;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Enums;
using AngularApi.Contracts.Models;
using AngularApi.Contracts.Services;
using AngularApi.Models;
using AngularApi.Options;
using AngularApi.Services;
using AngularApi.Contracts.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AngularApi.Services.impelementation;

public class AppointmentService : IAppointmentService
{
    private readonly MedicalCenterDbContext _context;
    private readonly AppointmentSettings _appointmentSettings;
    private readonly IEmailService _emailService;
    private readonly EmailTemplateService _emailTemplateService;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        MedicalCenterDbContext context,
        IOptions<AppointmentSettings> appointmentSettings,
        IEmailService emailService,
        EmailTemplateService emailTemplateService,
        ILogger<AppointmentService> logger)
    {
        _context = context;
        _appointmentSettings = appointmentSettings.Value;
        _emailService = emailService;
        _emailTemplateService = emailTemplateService;
        _logger = logger;
    }

    public Task<PagedResult<AppointmentDTO>> GetAppointmentsAsync(PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.Appointments.SelectAppointmentDto().ToPagedResultAsync(pagination, cancellationToken);

    public Task<PagedResult<AppointmentDTO>> GetAllAppointmentsAsync(PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.Appointments.SelectAppointmentDto().ToPagedResultAsync(pagination, cancellationToken);

    public Task<Appointment?> GetAppointmentByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Appointments.FindAsync([id], cancellationToken).AsTask();

    public async Task<bool> UpdateAppointmentAsync(int id, UpdateAppointmentDTO appointmentDto, CancellationToken cancellationToken = default)
    {
        var appointment = await _context.Appointments.FindAsync([id], cancellationToken);
        if (appointment == null) return false;
        appointment.AppointmentTakenDate = appointmentDto.AppointmentTakenDate;
        _context.Entry(appointment).State = EntityState.Modified;
        try { await _context.SaveChangesAsync(cancellationToken); return true; }
        catch (DbUpdateConcurrencyException) { return false; }
    }

    public async Task<decimal> GetTotalEarningsAsync(CancellationToken cancellationToken = default) =>
        await _context.Appointments.SumAsync(p => p.Amount, cancellationToken) ?? 0m;

    public async Task<(Appointment? Appointment, string? ErrorMessage)> CreateAppointmentAsync(
        CreateAppointmentDTO dto, string patientId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(dto.DoctorId)) return (null, "DoctorId is required");
        var doctor = await _context.Doctors.FindAsync([dto.DoctorId], cancellationToken);
        if (doctor == null) return (null, "Invalid DoctorId");
        var medicalCenter = await _context.MedicalCenter.FindAsync([dto.MedicalCenterId], cancellationToken);
        if (medicalCenter == null) return (null, "Invalid MedicalCenterId");

        var appointment = new Appointment
        {
            DoctorId = dto.DoctorId,
            DoctorName = doctor.Name,
            PatientId = patientId,
            MedicalCenterId = dto.MedicalCenterId,
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            AppointmentTakenDate = dto.AppointmentTakenDate,
            ProbableStartTime = dto.ProbableStartTime,
            AppointmentStatusId = (int)AppointmentStatusEnum.Active,
            Amount = medicalCenter.FirstConsultationFee ?? _appointmentSettings.DefaultFee,
            PaymentStatus = "Pending",
        };
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync(cancellationToken);
        await SendConfirmationEmailAsync(appointment);
        return (appointment, null);
    }

    private async Task SendConfirmationEmailAsync(Appointment appointment)
    {
        if (string.IsNullOrWhiteSpace(appointment.Email)) return;
        try
        {
            var body = _emailTemplateService.GetAppointmentConfirmationEmail(
                WebUtility.HtmlEncode(appointment.Name ?? "Patient"),
                WebUtility.HtmlEncode(appointment.DoctorName ?? "Doctor"),
                appointment.ProbableStartTime?.ToString("f") ?? appointment.AppointmentTakenDate?.ToString("f") ?? "");
            await _emailService.SendEmailAsync(new Message([appointment.Email], "Appointment Confirmation - CareShift", body));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send appointment confirmation email for appointment {AppointmentId}", appointment.Id);
        }
    }

    public async Task<bool> DeleteAppointmentAsync(int id, CancellationToken cancellationToken = default)
    {
        var appointment = await _context.Appointments.FindAsync([id], cancellationToken);
        if (appointment == null) return false;
        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<PagedResult<AppointmentDTO>> GetAppointmentsByPatientAsync(string patientId, PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.Appointments.Where(a => a.PatientId == patientId).SelectAppointmentDto().ToPagedResultAsync(pagination, cancellationToken);

    public Task<PagedResult<AppointmentDTO>> GetAppointmentsByDateAsync(DateTime date, PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.Appointments.Where(a => a.ProbableStartTime!.Value.Date == date.Date).SelectAppointmentDto().ToPagedResultAsync(pagination, cancellationToken);

    public Task<PagedResult<AppointmentDTO>> GetAppointmentsByStatusAsync(AppointmentStatusEnum status, PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.Appointments.Where(a => a.AppointmentStatus!.Status == status).SelectAppointmentDto().ToPagedResultAsync(pagination, cancellationToken);

    public Task<PagedResult<AppointmentDTO>> GetTodaysAppointmentsAsync(PaginationParameters pagination, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        return _context.Appointments.Where(a => a.ProbableStartTime!.Value.Date == today).SelectAppointmentDto().ToPagedResultAsync(pagination, cancellationToken);
    }

    public Task<PagedResult<AppointmentDTO>> GetUpcomingAppointmentsAsync(PaginationParameters pagination, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        return _context.Appointments.Where(a => a.ProbableStartTime > now).OrderBy(a => a.ProbableStartTime).SelectAppointmentDto().ToPagedResultAsync(pagination, cancellationToken);
    }

    public Task<PagedResult<AppointmentDTO>> GetAppointmentsByPatientAndStatusAsync(string patientId, AppointmentStatusEnum status, PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.Appointments.Where(a => a.PatientId == patientId && a.AppointmentStatus!.Status == status).SelectAppointmentDto().ToPagedResultAsync(pagination, cancellationToken);

    public Task<PagedResult<AppointmentDTO>> GetAppointmentHistoryByPatientAsync(string patientId, PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.Appointments.Where(a => a.PatientId == patientId).OrderByDescending(a => a.ProbableStartTime).SelectAppointmentDto().ToPagedResultAsync(pagination, cancellationToken);
}
