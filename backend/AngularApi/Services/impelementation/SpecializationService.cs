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

    public async Task<Specialization> CreateSpecializationAsync(Specialization specialization, CancellationToken cancellationToken = default)
    {
        _context.Specializations.Add(specialization);
        await _context.SaveChangesAsync(cancellationToken);
        return specialization;
    }

    public async Task<bool> UpdateSpecializationAsync(int id, Specialization specialization, CancellationToken cancellationToken = default)
    {
        if (id != specialization.Id) return false;
        var existing = await _context.Specializations.FindAsync([id], cancellationToken);
        if (existing == null) return false;
        existing.SpecializationName = specialization.SpecializationName;
        existing.SpecializationImage = specialization.SpecializationImage;
        existing.Description = specialization.Description;
        existing.IsActive = specialization.IsActive;
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
}
