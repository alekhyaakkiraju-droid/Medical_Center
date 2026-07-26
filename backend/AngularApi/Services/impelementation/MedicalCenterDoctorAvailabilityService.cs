using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Services.impelementation;

public class MedicalCenterDoctorAvailabilityService : IMedicalCenterDoctorAvailabilityService
{
    private readonly MedicalCenterDbContext _context;
#pragma warning disable IDE0052
    private readonly IOwnershipValidator _ownershipValidator;
#pragma warning restore IDE0052

    public MedicalCenterDoctorAvailabilityService(
        MedicalCenterDbContext context,
        IOwnershipValidator ownershipValidator)
    {
        _context = context;
        _ownershipValidator = ownershipValidator;
    }

    public Task<PagedResult<MedicalCenterDoctorAvailabilityDTO>> GetAllAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken = default) =>
        _context.MedicalCenterDoctorAvailability
            .Select(a => new MedicalCenterDoctorAvailabilityDTO
            {
                Id = a.Id,
                MedicalCenterId = a.MedicalCenterId,
                DayOfWeek = a.DayOfWeek,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                IsAvailable = a.IsAvailable,
                ReasonOfUnavailability = a.ReasonOfUnavailability
            })
            .ToPagedResultAsync(pagination, cancellationToken);

    public Task<MedicalCenterDoctorAvailability?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.MedicalCenterDoctorAvailability.FindAsync([id], cancellationToken).AsTask();

    public async Task<MedicalCenterDoctorAvailability?> CreateAsync(
        MedicalCenterDoctorAvailability availability,
        CancellationToken cancellationToken = default)
    {
        if (availability.MedicalCenterId.HasValue
            && !await MedicalCenterExistsAsync(availability.MedicalCenterId.Value, cancellationToken))
        {
            return null;
        }

        _context.MedicalCenterDoctorAvailability.Add(availability);
        await _context.SaveChangesAsync(cancellationToken);
        return availability;
    }

    public async Task<bool> UpdateAsync(
        int id,
        MedicalCenterDoctorAvailability availability,
        CancellationToken cancellationToken = default)
    {
        if (id != availability.Id)
        {
            return false;
        }

        _context.Entry(availability).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await AvailabilityExistsAsync(id, cancellationToken))
            {
                return false;
            }

            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var availability = await _context.MedicalCenterDoctorAvailability.FindAsync([id], cancellationToken);
        if (availability == null)
        {
            return false;
        }

        _context.MedicalCenterDoctorAvailability.Remove(availability);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private Task<bool> AvailabilityExistsAsync(int id, CancellationToken cancellationToken = default) =>
        _context.MedicalCenterDoctorAvailability.AnyAsync(e => e.Id == id, cancellationToken);

    private Task<bool> MedicalCenterExistsAsync(int medicalCenterId, CancellationToken cancellationToken = default) =>
        _context.MedicalCenter.AnyAsync(m => m.Id == medicalCenterId, cancellationToken);
}
