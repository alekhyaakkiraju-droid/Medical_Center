using System.Security.Claims;

namespace AngularApi.Services.Interfaces;

public interface IOwnershipValidator
{
    bool IsAdmin(ClaimsPrincipal user);

    bool CanAccessPatientResource(ClaimsPrincipal user, string patientId);

    bool CanAccessDoctorResource(ClaimsPrincipal user, string doctorId);
}
