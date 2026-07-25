using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Options;
using AngularApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AngularApi.Services.impelementation;

public class AppointmentService : IAppointmentService
{
    private readonly MedicalCenterDbContext _context;
    private readonly AppointmentSettings _appointmentSettings;

    public AppointmentService(MedicalCenterDbContext context, IOptions<AppointmentSettings> appointmentSettings)
    {
        _context = context;
        _appointmentSettings = appointmentSettings.Value;
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
        if (appointment == null)
        {
            return false;
        }

        appointment.AppointmentTakenDate = appointmentDto.AppointmentTakenDate;
        _context.Entry(appointment).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }

    public async Task<decimal> GetTotalEarningsAsync(CancellationToken cancellationToken = default) =>
        await _context.Appointments.SumAsync(p => p.Amount, cancellationToken) ?? 0m;

    public async Task<(Appointment? Appointment, string? ErrorMessage)> CreateAppointmentAsync(
        Appointment appointment,
        string patientId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(appointment.DoctorId))
        {
            return (null, "DoctorId is required");
        }

        var doctor = await _context.Doctors.FindAsync([appointment.DoctorId], cancellationToken);
        if (doctor == null)
        {
            return (null, "Invalid DoctorId");
        }

        appointment.DoctorName = doctor.Name;
        appointment.PatientId = patientId;
        appointment.MedicalCenterId = _appointmentSettings.DefaultCenterId;
        appointment.AppointmentStatusId = (int)AppointmentStatusEnum.Active;
        appointment.Amount = _appointmentSettings.DefaultFee;
        appointment.PaymentStatus = "Pending";

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync(cancellationToken);

        return (appointment, null);
    }

    public async Task<bool> DeleteAppointmentAsync(int id, CancellationToken cancellationToken = default)
    {
        var appointment = await _context.Appointments.FindAsync([id], cancellationToken);
        if (appointment == null)
        {
            return false;
        }

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<PagedResult<AppointmentDTO>> GetAppointmentsByPatientAsync(string patientId, PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.Appointments
            .Where(a => a.PatientId == patientId)
            .SelectAppointmentDto()
            .ToPagedResultAsync(pagination, cancellationToken);

    public Task<PagedResult<AppointmentDTO>> GetAppointmentsByDateAsync(DateTime date, PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.Appointments
            .Where(a => a.ProbableStartTime!.Value.Date == date.Date)
            .SelectAppointmentDto()
            .ToPagedResultAsync(pagination, cancellationToken);

    public Task<PagedResult<AppointmentDTO>> GetAppointmentsByStatusAsync(AppointmentStatusEnum status, PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.Appointments
            .Where(a => a.AppointmentStatus!.Status == status)
            .SelectAppointmentDto()
            .ToPagedResultAsync(pagination, cancellationToken);

    public Task<PagedResult<AppointmentDTO>> GetTodaysAppointmentsAsync(PaginationParameters pagination, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        return _context.Appointments
            .Where(a => a.ProbableStartTime!.Value.Date == today)
            .SelectAppointmentDto()
            .ToPagedResultAsync(pagination, cancellationToken);
    }

    public Task<PagedResult<AppointmentDTO>> GetUpcomingAppointmentsAsync(PaginationParameters pagination, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        return _context.Appointments
            .Where(a => a.ProbableStartTime > now)
            .OrderBy(a => a.ProbableStartTime)
            .SelectAppointmentDto()
            .ToPagedResultAsync(pagination, cancellationToken);
    }

    public Task<PagedResult<AppointmentDTO>> GetAppointmentsByPatientAndStatusAsync(
        string patientId,
        AppointmentStatusEnum status,
        PaginationParameters pagination,
        CancellationToken cancellationToken = default) =>
        _context.Appointments
            .Where(a => a.PatientId == patientId && a.AppointmentStatus!.Status == status)
            .SelectAppointmentDto()
            .ToPagedResultAsync(pagination, cancellationToken);

    public Task<PagedResult<AppointmentDTO>> GetAppointmentHistoryByPatientAsync(string patientId, PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.Appointments
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.ProbableStartTime)
            .SelectAppointmentDto()
            .ToPagedResultAsync(pagination, cancellationToken);
}
