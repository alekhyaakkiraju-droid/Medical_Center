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

    public Task<MedicalCenter?> GetMedicalCenterByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.MedicalCenter.FindAsync([id], cancellationToken).AsTask();

    public async Task<MedicalCenter> CreateMedicalCenterAsync(
        MedicalCenter medicalCenter,
        CancellationToken cancellationToken = default)
    {
        _context.MedicalCenter.Add(medicalCenter);
        await _context.SaveChangesAsync(cancellationToken);
        return medicalCenter;
    }

    public async Task<bool> UpdateMedicalCenterAsync(
        int id,
        MedicalCenter medicalCenter,
        CancellationToken cancellationToken = default)
    {
        if (id != medicalCenter.Id)
        {
            return false;
        }

        _context.Entry(medicalCenter).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await MedicalCenterExistsAsync(id, cancellationToken))
            {
                return false;
            }

            throw;
        }
    }

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
