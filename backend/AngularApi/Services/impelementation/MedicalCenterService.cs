using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Services.impelementation;

public class MedicalCenterService : IMedicalCenterService
{
    private readonly MedicalCenterDbContext _context;

    public MedicalCenterService(MedicalCenterDbContext context)
    {
        _context = context;
    }

    public Task<PagedResult<MedicalCenterListItemDTO>> GetMedicalCentersAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken = default) =>
        _context.MedicalCenter
            .Select(m => new MedicalCenterListItemDTO
            {
                Id = m.Id,
                HospitalAffiliationId = m.HospitalAffiliationId,
                TimeSlotPerClientInMin = m.TimeSlotPerClientInMin,
                FirstConsultationFee = m.FirstConsultationFee,
                FollowupConsultationFee = m.FollowupConsultationFee,
                StreetAddress = m.StreetAddress,
                City = m.City,
                State = m.State,
                Zip = m.Zip
            })
            .ToPagedResultAsync(pagination, cancellationToken);

    public Task<MedicalCenterDetailDTO?> GetMedicalCenterByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.MedicalCenter
            .Where(m => m.Id == id)
            .SelectMedicalCenterDetailDto()
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<MedicalCenter> CreateMedicalCenterAsync(CreateMedicalCenterDTO dto, CancellationToken cancellationToken = default)
    { var medicalCenter = MapToEntity(dto); _context.MedicalCenter.Add(medicalCenter); await _context.SaveChangesAsync(cancellationToken); return medicalCenter; }
    public async Task<bool> UpdateMedicalCenterAsync(int id, UpdateMedicalCenterDTO dto, CancellationToken cancellationToken = default)
    { var existing = await _context.MedicalCenter.FindAsync([id], cancellationToken); if (existing == null) return false; ApplyDto(existing, dto);
      try { await _context.SaveChangesAsync(cancellationToken); return true; } catch (DbUpdateConcurrencyException) { if (!await MedicalCenterExistsAsync(id, cancellationToken)) return false; throw; } }
    private static MedicalCenter MapToEntity(CreateMedicalCenterDTO dto) => new() { HospitalAffiliationId = dto.HospitalAffiliationId, TimeSlotPerClientInMin = dto.TimeSlotPerClientInMin, FirstConsultationFee = dto.FirstConsultationFee, FollowupConsultationFee = dto.FollowupConsultationFee, StreetAddress = dto.StreetAddress, City = dto.City, State = dto.State, Zip = dto.Zip };
    private static void ApplyDto(MedicalCenter entity, UpdateMedicalCenterDTO dto) { entity.HospitalAffiliationId = dto.HospitalAffiliationId; entity.TimeSlotPerClientInMin = dto.TimeSlotPerClientInMin; entity.FirstConsultationFee = dto.FirstConsultationFee; entity.FollowupConsultationFee = dto.FollowupConsultationFee; entity.StreetAddress = dto.StreetAddress; entity.City = dto.City; entity.State = dto.State; entity.Zip = dto.Zip; }

    public async Task<bool> DeleteMedicalCenterAsync(int id, CancellationToken cancellationToken = default)
    {
        var medicalCenter = await _context.MedicalCenter.FindAsync([id], cancellationToken);
        if (medicalCenter == null)
        {
            return false;
        }

        _context.MedicalCenter.Remove(medicalCenter);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private Task<bool> MedicalCenterExistsAsync(int id, CancellationToken cancellationToken = default) =>
        _context.MedicalCenter.AnyAsync(e => e.Id == id, cancellationToken);
}
