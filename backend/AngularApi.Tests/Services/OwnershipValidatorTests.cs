using AngularApi.Models;
using AngularApi.Contracts.Models;
using AngularApi.Services.impelementation;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AngularApi.Tests.Services;

public class OwnershipValidatorTests
{
    [Fact]
    public void CanAccessPatientResource_AdminUser_ReturnsTrue()
    {
        using var context = CreateDbContext();
        var validator = new OwnershipValidator(context);
        var user = CreateUser("admin-user", "admin");

        validator.CanAccessPatientResource(user, "other-patient").Should().BeTrue();
    }

    [Fact]
    public void CanAccessPatientResource_OwnResource_ReturnsTrue()
    {
        using var context = CreateDbContext();
        var validator = new OwnershipValidator(context);
        var user = CreateUser("patient-1", "user");

        validator.CanAccessPatientResource(user, "patient-1").Should().BeTrue();
    }

    [Fact]
    public void CanAccessPatientResource_OtherPatient_ReturnsFalse()
    {
        using var context = CreateDbContext();
        var validator = new OwnershipValidator(context);
        var user = CreateUser("patient-1", "user");

        validator.CanAccessPatientResource(user, "patient-2").Should().BeFalse();
    }

    [Fact]
    public void CanAccessDoctorResource_OtherDoctor_ReturnsFalse()
    {
        using var context = CreateDbContext();
        var validator = new OwnershipValidator(context);
        var user = CreateUser("doctor-1", "doctor");

        validator.CanAccessDoctorResource(user, "doctor-2").Should().BeFalse();
    }

    [Fact]
    public void CanAccessPatientReviewResource_OwnReview_ReturnsTrue()
    {
        using var context = CreateDbContext();
        var validator = new OwnershipValidator(context);
        validator.CanAccessPatientReviewResource(CreateUser("patient-1", "user"), "patient-1").Should().BeTrue();
    }

    [Fact]
    public void CanAccessPatientReviewResource_OtherReview_ReturnsFalse()
    {
        using var context = CreateDbContext();
        var validator = new OwnershipValidator(context);
        validator.CanAccessPatientReviewResource(CreateUser("patient-1", "user"), "patient-2").Should().BeFalse();
    }

    [Fact]
    public void CanAccessPatientReviewResource_AdminUser_ReturnsTrue()
    {
        using var context = CreateDbContext();
        var validator = new OwnershipValidator(context);
        validator.CanAccessPatientReviewResource(CreateUser("admin-user", "admin"), "patient-2").Should().BeTrue();
    }

    [Fact]
    public void CanAccessPatientReviewResource_EmptyPatientId_ReturnsFalse()
    {
        using var context = CreateDbContext();
        var validator = new OwnershipValidator(context);
        validator.CanAccessPatientReviewResource(CreateUser("patient-1", "user"), string.Empty).Should().BeFalse();
    }

    [Fact]
    public void CanAccessMedicalCenterResource_AdminUser_ReturnsTrue()
    {
        using var context = CreateDbContext();
        var validator = new OwnershipValidator(context);
        validator.CanAccessMedicalCenterResource(CreateUser("admin-user", "admin")).Should().BeTrue();
    }

    [Fact]
    public void CanAccessMedicalCenterResource_NonAdminUser_ReturnsFalse()
    {
        using var context = CreateDbContext();
        var validator = new OwnershipValidator(context);
        validator.CanAccessMedicalCenterResource(CreateUser("doctor-1", "doctor")).Should().BeFalse();
    }

    [Fact]
    public async Task CanAccessAppointmentResource_PatientOwner_ReturnsTrue()
    {
        await using var context = CreateDbContext();
        var appointmentId = await SeedAppointmentAsync(context, "patient-1", "doctor-1");
        var validator = new OwnershipValidator(context);

        var result = await validator.CanAccessAppointmentResource(CreateUser("patient-1", "user"), appointmentId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAccessAppointmentResource_AssignedDoctor_ReturnsTrue()
    {
        await using var context = CreateDbContext();
        var appointmentId = await SeedAppointmentAsync(context, "patient-1", "doctor-1");
        var validator = new OwnershipValidator(context);

        var result = await validator.CanAccessAppointmentResource(CreateUser("doctor-1", "doctor"), appointmentId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAccessAppointmentResource_AdminUser_ReturnsTrue()
    {
        await using var context = CreateDbContext();
        var appointmentId = await SeedAppointmentAsync(context, "patient-1", "doctor-1");
        var validator = new OwnershipValidator(context);

        var result = await validator.CanAccessAppointmentResource(CreateUser("admin-user", "admin"), appointmentId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAccessAppointmentResource_NonOwnerPatient_ReturnsFalse()
    {
        await using var context = CreateDbContext();
        var appointmentId = await SeedAppointmentAsync(context, "patient-1", "doctor-1");
        var validator = new OwnershipValidator(context);

        var result = await validator.CanAccessAppointmentResource(CreateUser("patient-2", "user"), appointmentId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanAccessAppointmentResource_NonAssignedDoctor_ReturnsFalse()
    {
        await using var context = CreateDbContext();
        var appointmentId = await SeedAppointmentAsync(context, "patient-1", "doctor-1");
        var validator = new OwnershipValidator(context);

        var result = await validator.CanAccessAppointmentResource(CreateUser("doctor-2", "doctor"), appointmentId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanAccessAppointmentResource_NonExistentAppointment_ReturnsFalse()
    {
        await using var context = CreateDbContext();
        var validator = new OwnershipValidator(context);

        var result = await validator.CanAccessAppointmentResource(CreateUser("patient-1", "user"), 9999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanAccessAppointmentResource_UnassignedDoctorOnlyPatientAndAdminAllowed()
    {
        await using var context = CreateDbContext();
        var appointmentId = await SeedAppointmentAsync(context, "patient-1", doctorId: null);
        var validator = new OwnershipValidator(context);

        (await validator.CanAccessAppointmentResource(CreateUser("patient-1", "user"), appointmentId)).Should().BeTrue();
        (await validator.CanAccessAppointmentResource(CreateUser("doctor-1", "doctor"), appointmentId)).Should().BeFalse();
        (await validator.CanAccessAppointmentResource(CreateUser("admin-user", "admin"), appointmentId)).Should().BeTrue();
    }

    private static async Task<int> SeedAppointmentAsync(
        MedicalCenterDbContext context,
        string patientId,
        string? doctorId)
    {
        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            DoctorName = doctorId ?? "Unassigned",
            AppointmentTakenDate = DateTime.UtcNow,
        };

        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();
        return appointment.Id;
    }

    private static MedicalCenterDbContext CreateDbContext()
    {
        return new MedicalCenterDbContext(
            new DbContextOptionsBuilder<MedicalCenterDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
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
