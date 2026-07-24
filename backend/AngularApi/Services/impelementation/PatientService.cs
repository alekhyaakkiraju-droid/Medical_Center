using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Services.impelementation;

public class PatientService : IPatientService
{
    private readonly MedicalCenterDbContext _context;

    public PatientService(MedicalCenterDbContext context)
    {
        _context = context;
    }

    public Task<PagedResult<PatientDTO>> GetAllPatientsWithReviewsAsync(PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.Patients
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
            .ToPagedResultAsync(pagination, cancellationToken);

    public Task<PatientDTO?> GetPatientByIdAsync(string id, CancellationToken cancellationToken = default) =>
        _context.Patients
            .Where(p => p.Id == id)
            .Select(p => new PatientDTO
            {
                PatientId = p.Id,
                Name = p.Name,
                Email = p.Email,
                Image = p.Image
            })
            .FirstOrDefaultAsync(cancellationToken);

    public Task<PagedResult<AppointmentDTO>> GetPatientAppointmentsAsync(string patientId, PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.Appointments
            .Where(a => a.PatientId == patientId)
            .SelectAppointmentDto()
            .ToPagedResultAsync(pagination, cancellationToken);

    public Task<PagedResult<AppointmentDTO>> GetAppointmentsByDateRangeAsync(
        string patientId,
        DateTime startDate,
        DateTime endDate,
        PaginationParameters pagination,
        CancellationToken cancellationToken = default) =>
        _context.Appointments
            .Where(a => a.PatientId == patientId && a.AppointmentTakenDate >= startDate && a.AppointmentTakenDate <= endDate)
            .SelectAppointmentDto()
            .ToPagedResultAsync(pagination, cancellationToken);

    public async Task UpdateReviewAsync(PatientReview review, CancellationToken cancellationToken = default)
    {
        _context.Entry(review).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeletePatientAppointmentAsync(string patientId, int appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patientId, cancellationToken);
        if (appointment == null)
        {
            return false;
        }

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdatePatientAsync(string id, PatientDTO model, CancellationToken cancellationToken = default)
    {
        var patient = await _context.Patients.FindAsync([id], cancellationToken);
        if (patient == null)
        {
            return false;
        }

        patient.Name = model.Name;
        patient.Email = model.Email;
        patient.Image = model.Image;

        _context.Patients.Update(patient);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeletePatientAsync(string id, CancellationToken cancellationToken = default)
    {
        var patient = await _context.Patients.FindAsync([id], cancellationToken);
        if (patient == null)
        {
            return false;
        }

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
