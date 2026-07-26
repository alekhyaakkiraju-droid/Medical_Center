using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace AngularApi.Services.impelementation;

public class DoctorService : IDoctorService
{
    private readonly MedicalCenterDbContext _context;
    private readonly ResiliencePipeline _retryPipeline;

    public DoctorService(MedicalCenterDbContext context)
    {
        _context = context;
        _retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                DelayGenerator = static args =>
                    new ValueTask<TimeSpan?>(TimeSpan.FromMilliseconds(1000 * args.AttemptNumber)),
                ShouldHandle = new PredicateBuilder().Handle<Exception>()
            })
            .Build();
    }

    public Task<PagedResult<DoctorDTO>> GetDoctorsAsync(PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.Doctors.SelectDoctorDto().ToPagedResultAsync(pagination, cancellationToken);

    public Task<PagedResult<DoctorDTO>> GetDoctorsWithSpecializationAsync(PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.Doctors.SelectDoctorDto().ToPagedResultAsync(pagination, cancellationToken);

    public Task<Doctor?> GetDoctorByIdAsync(string doctorId, CancellationToken cancellationToken = default) =>
        _retryPipeline.ExecuteAsync(
            async ct => await _context.Doctors.FindAsync([doctorId], ct),
            cancellationToken).AsTask();

    public async Task<Doctor> CreateDoctorAsync(Doctor doctor, CancellationToken cancellationToken = default)
    {
        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync(cancellationToken);
        return doctor;
    }

    public Task<PagedResult<BookingDTO>> GetBookingsAsync(string doctorId, PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _retryPipeline.ExecuteAsync(
            async ct => await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                            a.AppointmentStatus!.Status == AppointmentStatusEnum.Active)
                .SelectBookingDto()
                .ToPagedResultAsync(pagination, ct),
            cancellationToken).AsTask();

    public Task<PagedResult<BookingDTO>> GetBookingsByStatusAsync(string doctorId, AppointmentStatusEnum status, PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.Appointments
            .Where(a => a.DoctorId == doctorId && a.AppointmentStatus!.Status == status)
            .SelectBookingDto()
            .ToPagedResultAsync(pagination, cancellationToken);

    public Task<PagedResult<BookingDTO>> GetTodaysBookingsAsync(string doctorId, PaginationParameters pagination, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        return _context.Appointments
            .Where(a => a.DoctorId == doctorId && a.AppointmentTakenDate == today && a.AppointmentStatus!.Status == AppointmentStatusEnum.Active)
            .SelectBookingDto()
            .ToPagedResultAsync(pagination, cancellationToken);
    }

    public Task<PagedResult<BookingDTO>> GetUpcomingBookingsAsync(string doctorId, PaginationParameters pagination, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        return _context.Appointments
            .Where(a => a.DoctorId == doctorId && a.AppointmentTakenDate >= today && a.AppointmentStatus!.Status == AppointmentStatusEnum.Active)
            .SelectBookingDto()
            .ToPagedResultAsync(pagination, cancellationToken);
    }

    public Task<PagedResult<BookingDTO>> GetLast30DaysBookingsAsync(string doctorId, PaginationParameters pagination, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var thirtyDaysAgo = today.AddDays(-30);

        return _context.Appointments
            .Where(a => a.DoctorId == doctorId
                && a.AppointmentTakenDate >= thirtyDaysAgo
                && a.AppointmentTakenDate <= today
                && a.AppointmentStatus!.Status == AppointmentStatusEnum.Active)
            .SelectBookingDto()
            .ToPagedResultAsync(pagination, cancellationToken);
    }

    public Task<PagedResult<ReviewDTO>> GetReviewsAsync(string doctorId, PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.PatientReviews
            .Where(r => r.DoctorId == doctorId)
            .Select(r => new ReviewDTO
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
            })
            .ToPagedResultAsync(pagination, cancellationToken);

    public async Task<double> GetRatingAsync(string doctorId, CancellationToken cancellationToken = default) =>
        await _context.PatientReviews
            .Where(r => r.DoctorId == doctorId)
            .AverageAsync(r => (double?)r.OverallRating, cancellationToken) ?? 0d;

    public Task<PagedResult<DoctorQualificationDTO>> GetQualificationsAsync(string doctorId, PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.DoctorQualifications
            .Where(q => q.DoctorId == doctorId)
            .Select(q => new DoctorQualificationDTO
            {
                Id = q.Id,
                DoctorId = q.DoctorId,
                QualificationName = q.QualificationName,
                InstituteName = q.InstituteName,
                ProcurementYear = q.ProcurementYear
            })
            .ToPagedResultAsync(pagination, cancellationToken);

    public Task<PagedResult<string>> GetSpecializationsAsync(string doctorId, PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.DoctorSpecialization
            .Where(s => s.DoctorId == doctorId)
            .Select(s => s.Specialization!.SpecializationName!)
            .ToPagedResultAsync(pagination, cancellationToken);

    public async Task<bool> UpdateDoctorAsync(string id, Doctor doctor, CancellationToken cancellationToken = default)
    {
        if (id != doctor.Id)
        {
            return false;
        }

        _context.Entry(doctor).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await DoctorExistsAsync(id, cancellationToken))
            {
                return false;
            }

            throw;
        }
    }

    public async Task<bool> UpdateBookingAsync(int bookingId, Appointment updatedBooking, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Appointments.FindAsync([bookingId], cancellationToken);
        if (booking == null)
        {
            return false;
        }

        booking.AppointmentStatus = updatedBooking.AppointmentStatus;
        booking.AppointmentTakenDate = updatedBooking.AppointmentTakenDate;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteDoctorAsync(string id, CancellationToken cancellationToken = default)
    {
        var doctor = await _context.Doctors.FindAsync([id], cancellationToken);
        if (doctor == null)
        {
            return false;
        }

        _context.Doctors.Remove(doctor);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> CancelDoctorAppointmentAsync(string doctorId, int appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await _context.Appointments
            .Include(a => a.AppointmentStatus)
            .Where(a => a.DoctorId == doctorId && a.Id == appointmentId)
            .FirstOrDefaultAsync(cancellationToken);

        if (appointment == null)
        {
            return false;
        }

        appointment.AppointmentStatusId = (int)AppointmentStatusEnum.Canceled;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private Task<bool> DoctorExistsAsync(string id, CancellationToken cancellationToken = default) =>
        _context.Doctors.AnyAsync(e => e.Id == id, cancellationToken);
}
