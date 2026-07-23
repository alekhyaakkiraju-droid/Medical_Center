using AngularApi.Services.impelementation;
using FluentAssertions;
using System.Security.Claims;

namespace AngularApi.Tests.Services;

public class OwnershipValidatorTests
{
    private readonly OwnershipValidator _validator = new();

    [Fact]
    public void CanAccessPatientResource_AdminUser_ReturnsTrue()
    {
        var user = CreateUser("admin-user", "admin");

        _validator.CanAccessPatientResource(user, "other-patient").Should().BeTrue();
    }

    [Fact]
    public void CanAccessPatientResource_OwnResource_ReturnsTrue()
    {
        var user = CreateUser("patient-1", "user");

        _validator.CanAccessPatientResource(user, "patient-1").Should().BeTrue();
    }

    [Fact]
    public void CanAccessPatientResource_OtherPatient_ReturnsFalse()
    {
        var user = CreateUser("patient-1", "user");

        _validator.CanAccessPatientResource(user, "patient-2").Should().BeFalse();
    }

    [Fact]
    public void CanAccessDoctorResource_OtherDoctor_ReturnsFalse()
    {
        var user = CreateUser("doctor-1", "doctor");

        _validator.CanAccessDoctorResource(user, "doctor-2").Should().BeFalse();
    }

    private static ClaimsPrincipal CreateUser(string userId, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role),
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }
}
