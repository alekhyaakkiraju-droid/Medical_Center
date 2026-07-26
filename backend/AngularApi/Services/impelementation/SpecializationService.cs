using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Services.impelementation;

public class SpecializationService : ISpecializationService
{
    private readonly MedicalCenterDbContext _context;
    public SpecializationService(MedicalCenterDbContext context) => _context = context;

    public Task<PagedResult<SpecializationListItemDTO>> GetSpecializationsAsync(PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.Specializations.Select(s => new SpecializationListItemDTO
        {
            Id = s.Id,
            SpecializationName = s.SpecializationName,
            SpecializationImage = s.SpecializationImage,
            Description = s.Description,
            IsActive = s.IsActive
        }).ToPagedResultAsync(pagination, cancellationToken);

    public Task<Specialization?> GetSpecializationByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Specializations.Include(s => s.Services).FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<Specialization> CreateSpecializationAsync(CreateSpecializationDTO dto, CancellationToken cancellationToken = default)
    {
        var specialization = MapToEntity(dto);
        _context.Specializations.Add(specialization);
        await _context.SaveChangesAsync(cancellationToken);
        return specialization;
    }

    public async Task<bool> UpdateSpecializationAsync(int id, UpdateSpecializationDTO dto, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Specializations.FindAsync([id], cancellationToken);
        if (existing == null) return false;

        existing.SpecializationName = dto.SpecializationName;
        existing.SpecializationImage = dto.SpecializationImage;
        existing.Description = dto.Description;
        existing.IsActive = dto.IsActive;

        try { await _context.SaveChangesAsync(cancellationToken); return true; }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Specializations.AnyAsync(e => e.Id == id, cancellationToken)) return false;
            throw;
        }
    }

    public async Task<bool> DeleteSpecializationAsync(int id, CancellationToken cancellationToken = default)
    {
        var specialization = await _context.Specializations.FindAsync([id], cancellationToken);
        if (specialization == null) return false;
        _context.Specializations.Remove(specialization);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static Specialization MapToEntity(CreateSpecializationDTO dto) => new()
    {
        SpecializationName = dto.SpecializationName,
        SpecializationImage = dto.SpecializationImage,
        Description = dto.Description,
        IsActive = dto.IsActive
    };
}
