using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;
using AngularApi.Contracts.Services.Interfaces;
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

    public Task<MedicalCenterDoctorAvailabilityDetailDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.MedicalCenterDoctorAvailability
            .Where(a => a.Id == id)
            .SelectMedicalCenterDoctorAvailabilityDetailDto()
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<MedicalCenterDoctorAvailability?> CreateAsync(CreateMedicalCenterDoctorAvailabilityDTO dto, CancellationToken cancellationToken = default)
    { if (!await MedicalCenterExistsAsync(dto.MedicalCenterId, cancellationToken)) return null; var availability = MapToEntity(dto); _context.MedicalCenterDoctorAvailability.Add(availability); await _context.SaveChangesAsync(cancellationToken); return availability; }
    public async Task<bool> UpdateAsync(int id, UpdateMedicalCenterDoctorAvailabilityDTO dto, CancellationToken cancellationToken = default)
    { var existing = await _context.MedicalCenterDoctorAvailability.FindAsync([id], cancellationToken); if (existing == null) return false; if (!await MedicalCenterExistsAsync(dto.MedicalCenterId, cancellationToken)) return false; ApplyDto(existing, dto);
      try { await _context.SaveChangesAsync(cancellationToken); return true; } catch (DbUpdateConcurrencyException) { if (!await AvailabilityExistsAsync(id, cancellationToken)) return false; throw; } }
    private static MedicalCenterDoctorAvailability MapToEntity(CreateMedicalCenterDoctorAvailabilityDTO dto) => new() { MedicalCenterId = dto.MedicalCenterId, DayOfWeek = dto.DayOfWeek, StartTime = dto.StartTime, EndTime = dto.EndTime, IsAvailable = dto.IsAvailable, ReasonOfUnavailability = dto.ReasonOfUnavailability };
    private static void ApplyDto(MedicalCenterDoctorAvailability entity, UpdateMedicalCenterDoctorAvailabilityDTO dto) { entity.MedicalCenterId = dto.MedicalCenterId; entity.DayOfWeek = dto.DayOfWeek; entity.StartTime = dto.StartTime; entity.EndTime = dto.EndTime; entity.IsAvailable = dto.IsAvailable; entity.ReasonOfUnavailability = dto.ReasonOfUnavailability; }

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
