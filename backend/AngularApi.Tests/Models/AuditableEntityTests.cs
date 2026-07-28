using AngularApi.Models;
using AngularApi.Contracts.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Tests.Models;

public class AuditableEntityTests
{
    private static MedicalCenterDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MedicalCenterDbContext(options);
    }

    [Fact]
    public async Task SaveChangesAsync_SetsCreatedAt_WhenAuditableEntityIsAdded()
    {
        await using var context = CreateContext();
        var beforeSave = DateTime.UtcNow;

        context.Appointments.Add(new Appointment
        {
            Name = "Test Appointment"
        });
        await context.SaveChangesAsync();

        var appointment = await context.Appointments.SingleAsync();
        appointment.CreatedAt.Should().BeOnOrAfter(beforeSave);
        appointment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        appointment.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_SetsUpdatedAt_WhenAuditableEntityIsModified()
    {
        await using var context = CreateContext();

        var appointment = new Appointment
        {
            Name = "Original Name"
        };
        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();

        var originalCreatedAt = appointment.CreatedAt;
        appointment.Name = "Updated Name";
        var beforeUpdate = DateTime.UtcNow;
        await context.SaveChangesAsync();

        appointment.CreatedAt.Should().Be(originalCreatedAt);
        appointment.UpdatedAt.Should().NotBeNull();
        appointment.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
        appointment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Appointments_HasCompositeIndexOnDoctorIdAndAppointmentTakenDate()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(Appointment));
        entityType.Should().NotBeNull();

        var indexPropertyNames = entityType!.GetIndexes()
            .Select(index => index.Properties.Select(property => property.Name).ToArray())
            .ToList();

        indexPropertyNames.Should().Contain(index => index.SequenceEqual(new[] { "DoctorId", "AppointmentTakenDate" }));
    }

    [Fact]
    public void Appointments_HasCompositeIndexOnPatientIdAndAppointmentTakenDate()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(Appointment));
        entityType.Should().NotBeNull();

        var indexPropertyNames = entityType!.GetIndexes()
            .Select(index => index.Properties.Select(property => property.Name).ToArray())
            .ToList();

        indexPropertyNames.Should().Contain(index => index.SequenceEqual(new[] { "PatientId", "AppointmentTakenDate" }));
    }

    [Fact]
    public void PatientReviews_HasIndexOnDoctorId()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(PatientReview));
        entityType.Should().NotBeNull();

        var indexPropertyNames = entityType!.GetIndexes()
            .Select(index => index.Properties.Select(property => property.Name).ToArray())
            .ToList();

        indexPropertyNames.Should().Contain(index => index.SequenceEqual(new[] { "DoctorId" }));
    }
}
