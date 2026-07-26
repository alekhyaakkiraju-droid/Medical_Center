using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Services.impelementation;

public class AppointmentStatusService : IAppointmentStatusService
{
    private readonly MedicalCenterDbContext _context;

    public AppointmentStatusService(MedicalCenterDbContext context)
    {
        _context = context;
    }

    public Task<PagedResult<AppointmentStatusListItemDTO>> GetAllAsync(PaginationParameters pagination, CancellationToken cancellationToken = default) =>
        _context.AppointmentStatus
            .Select(s => new AppointmentStatusListItemDTO
            {
                Id = s.Id,
                Status = s.Status
            })
            .ToPagedResultAsync(pagination, cancellationToken);

    public Task<AppointmentStatus?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.AppointmentStatus.FindAsync([id], cancellationToken).AsTask();

    public async Task<AppointmentStatus> CreateAsync(AppointmentStatus appointmentStatus, CancellationToken cancellationToken = default)
    {
        _context.AppointmentStatus.Add(appointmentStatus);
        await _context.SaveChangesAsync(cancellationToken);
        return appointmentStatus;
    }

    public async Task<bool> UpdateAsync(int id, AppointmentStatus appointmentStatus, CancellationToken cancellationToken = default)
    {
        if (id != appointmentStatus.Id)
        {
            return false;
        }

        var existing = await _context.AppointmentStatus.FindAsync([id], cancellationToken);
        if (existing == null)
        {
            return false;
        }

        existing.Status = appointmentStatus.Status;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ExistsAsync(id, cancellationToken))
            {
                return false;
            }

            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var appointmentStatus = await _context.AppointmentStatus.FindAsync([id], cancellationToken);
        if (appointmentStatus == null)
        {
            return false;
        }

        _context.AppointmentStatus.Remove(appointmentStatus);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) =>
        _context.AppointmentStatus.AnyAsync(e => e.Id == id, cancellationToken);
}
