using AngularApi.Services.Interfaces;
using System.Security.Claims;
namespace AngularApi.Services.impelementation;
public class OwnershipValidator : IOwnershipValidator {
    public bool IsAdmin(ClaimsPrincipal user) => user.IsInRole("admin");
    public bool CanAccessPatientResource(ClaimsPrincipal user, string patientId) { if (IsAdmin(user)) return true; var userId = user.FindFirstValue(ClaimTypes.NameIdentifier); return !string.IsNullOrEmpty(userId) && string.Equals(userId, patientId, StringComparison.Ordinal); }
    public bool CanAccessDoctorResource(ClaimsPrincipal user, string doctorId) { if (IsAdmin(user)) return true; var userId = user.FindFirstValue(ClaimTypes.NameIdentifier); return !string.IsNullOrEmpty(userId) && string.Equals(userId, doctorId, StringComparison.Ordinal); }
    public bool CanAccessPatientReviewResource(ClaimsPrincipal user, string reviewPatientId) { if (string.IsNullOrEmpty(reviewPatientId)) return false; return CanAccessPatientResource(user, reviewPatientId); }
    public bool CanAccessMedicalCenterResource(ClaimsPrincipal user) => IsAdmin(user);
}
