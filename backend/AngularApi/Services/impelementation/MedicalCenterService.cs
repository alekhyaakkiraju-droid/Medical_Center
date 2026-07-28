using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;
using AngularApi.Contracts.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AngularApi.Services.impelementation;

public class MedicalCenterService : IMedicalCenterService
{
    private readonly MedicalCenterDbContext _context;
    private readonly IOwnershipValidator _ownershipValidator;
    private readonly ILogger<MedicalCenterService> _logger;

    public MedicalCenterService(MedicalCenterDbContext context, IOwnershipValidator ownershipValidator, ILogger<MedicalCenterService> logger)
    {
        _context = context;
        _ownershipValidator = ownershipValidator;
        _logger = logger;
    }

    public Task<PagedResult<MedicalCenterListItemDTO>> GetMedicalCentersAsync(PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.MedicalCenter.Select(m => new MedicalCenterListItemDTO { Id = m.Id, HospitalAffiliationId = m.HospitalAffiliationId, TimeSlotPerClientInMin = m.TimeSlotPerClientInMin, FirstConsultationFee = m.FirstConsultationFee, FollowupConsultationFee = m.FollowupConsultationFee, StreetAddress = m.StreetAddress, City = m.City, State = m.State, Zip = m.Zip }).ToPagedResultAsync(pagination, cancellationToken);

    public Task<MedicalCenterDetailDTO?> GetMedicalCenterByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.MedicalCenter
            .Where(m => m.Id == id)
            .SelectMedicalCenterDetailDto()
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<(MedicalCenter? Center, ResourceMutationResult Result)> CreateMedicalCenterAsync(CreateMedicalCenterDTO dto, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        if (!_ownershipValidator.CanAccessMedicalCenterResource(user)) { LogDenial(user, "MedicalCenter", "new", "Create"); return (null, ResourceMutationResult.Forbidden); }
        var medicalCenter = MapToEntity(dto); _context.MedicalCenter.Add(medicalCenter); await _context.SaveChangesAsync(cancellationToken); return (medicalCenter, ResourceMutationResult.Success);
    }

    public async Task<ResourceMutationResult> UpdateMedicalCenterAsync(int id, UpdateMedicalCenterDTO dto, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        if (!_ownershipValidator.CanAccessMedicalCenterResource(user)) { LogDenial(user, "MedicalCenter", id.ToString(), "Update"); return ResourceMutationResult.Forbidden; }
        var existing = await _context.MedicalCenter.FindAsync([id], cancellationToken); if (existing == null) return ResourceMutationResult.NotFound;
        ApplyDto(existing, dto);
        try { await _context.SaveChangesAsync(cancellationToken); return ResourceMutationResult.Success; }
        catch (DbUpdateConcurrencyException) { if (!await MedicalCenterExistsAsync(id, cancellationToken)) return ResourceMutationResult.NotFound; throw; }
    }

    public async Task<ResourceMutationResult> DeleteMedicalCenterAsync(int id, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        if (!_ownershipValidator.CanAccessMedicalCenterResource(user)) { LogDenial(user, "MedicalCenter", id.ToString(), "Delete"); return ResourceMutationResult.Forbidden; }
        var medicalCenter = await _context.MedicalCenter.FindAsync([id], cancellationToken); if (medicalCenter == null) return ResourceMutationResult.NotFound;
        _context.MedicalCenter.Remove(medicalCenter); await _context.SaveChangesAsync(cancellationToken); return ResourceMutationResult.Success;
    }

    private static MedicalCenter MapToEntity(CreateMedicalCenterDTO dto) => new() { HospitalAffiliationId = dto.HospitalAffiliationId, TimeSlotPerClientInMin = dto.TimeSlotPerClientInMin, FirstConsultationFee = dto.FirstConsultationFee, FollowupConsultationFee = dto.FollowupConsultationFee, StreetAddress = dto.StreetAddress, City = dto.City, State = dto.State, Zip = dto.Zip };
    private static void ApplyDto(MedicalCenter entity, UpdateMedicalCenterDTO dto) { entity.HospitalAffiliationId = dto.HospitalAffiliationId; entity.TimeSlotPerClientInMin = dto.TimeSlotPerClientInMin; entity.FirstConsultationFee = dto.FirstConsultationFee; entity.FollowupConsultationFee = dto.FollowupConsultationFee; entity.StreetAddress = dto.StreetAddress; entity.City = dto.City; entity.State = dto.State; entity.Zip = dto.Zip; }
    private Task<bool> MedicalCenterExistsAsync(int id, CancellationToken cancellationToken = default) => _context.MedicalCenter.AnyAsync(e => e.Id == id, cancellationToken);
    private void LogDenial(ClaimsPrincipal user, string resourceType, string resourceId, string action) => _logger.LogWarning("Ownership validation denied {ActorId} {ResourceType} {ResourceId} {Action} {Result}", user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown", resourceType, resourceId, action, "Denied");
}
