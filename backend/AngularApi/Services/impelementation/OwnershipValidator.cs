using AngularApi.Models;
using AngularApi.Contracts.Models;
using AngularApi.Contracts.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AngularApi.Services.impelementation;

public class OwnershipValidator : IOwnershipValidator
{
    private readonly MedicalCenterDbContext _dbContext;

    public OwnershipValidator(MedicalCenterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public bool IsAdmin(ClaimsPrincipal user) => user.IsInRole("admin");

    public bool CanAccessPatientResource(ClaimsPrincipal user, string patientId)
    {
        if (IsAdmin(user))
        {
            return true;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrEmpty(userId)
            && string.Equals(userId, patientId, StringComparison.Ordinal);
    }

    public bool CanAccessDoctorResource(ClaimsPrincipal user, string doctorId)
    {
        if (IsAdmin(user))
        {
            return true;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrEmpty(userId)
            && string.Equals(userId, doctorId, StringComparison.Ordinal);
    }

    public bool CanAccessPatientReviewResource(ClaimsPrincipal user, string reviewPatientId)
    {
        if (string.IsNullOrEmpty(reviewPatientId))
        {
            return false;
        }

        return CanAccessPatientResource(user, reviewPatientId);
    }

    public bool CanAccessMedicalCenterResource(ClaimsPrincipal user) => IsAdmin(user);

    public async Task<bool> CanAccessAppointmentResource(ClaimsPrincipal user, int appointmentId)
    {
        if (IsAdmin(user))
        {
            return true;
        }

        var appointment = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.Id == appointmentId)
            .Select(a => new { a.PatientId, a.DoctorId })
            .FirstOrDefaultAsync();

        if (appointment == null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(appointment.PatientId))
        {
            return false;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        if (string.Equals(userId, appointment.PatientId, StringComparison.Ordinal))
        {
            return true;
        }

        if (!string.IsNullOrEmpty(appointment.DoctorId)
            && string.Equals(userId, appointment.DoctorId, StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }
}
