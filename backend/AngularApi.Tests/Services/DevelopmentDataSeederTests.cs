using AngularApi.Models;
using AngularApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Services;

public class DevelopmentDataSeederTests
{
    private static ServiceProvider CreateProvider()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => new MedicalCenterDbContext(options));
        services.AddIdentity<AppUser, IdentityRole>()
            .AddEntityFrameworkStores<MedicalCenterDbContext>()
            .AddDefaultTokenProviders();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task SeedAsync_AddsSpecializationsWhenDatabaseIsEmpty()
    {
        await using var provider = CreateProvider();
        await DevelopmentDataSeeder.SeedAsync(provider);

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        var count = await context.Specializations.CountAsync();
        count.Should().Be(3);
    }

    [Fact]
    public async Task SeedAsync_IsIdempotentWhenSpecializationsExist()
    {
        await using var provider = CreateProvider();

        await DevelopmentDataSeeder.SeedAsync(provider);
        await DevelopmentDataSeeder.SeedAsync(provider);

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        var count = await context.Specializations.CountAsync();
        count.Should().Be(3);
    }

    [Fact]
    public async Task SeedAsync_SeedsMedicalCenterWithExpectedFields()
    {
        await using var provider = CreateProvider();
        await DevelopmentDataSeeder.SeedAsync(provider);

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        var center = await context.MedicalCenter.SingleAsync();
        center.Id.Should().Be(DevelopmentDataSeeder.DefaultMedicalCenterId);
        center.TimeSlotPerClientInMin.Should().Be(30);
        center.FirstConsultationFee.Should().Be(50.00m);
        center.FollowupConsultationFee.Should().Be(30.00m);
        center.StreetAddress.Should().Be("450 CareShift Medical Plaza");
        center.City.Should().Be("Springfield");
        center.State.Should().Be("IL");
        center.Zip.Should().Be("62701");
    }

    [Fact]
    public async Task SeedAsync_SeedsAvailabilitySlotsForDefaultMedicalCenter()
    {
        await using var provider = CreateProvider();
        await DevelopmentDataSeeder.SeedAsync(provider);

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        var availability = await context.MedicalCenterDoctorAvailability
            .Where(a => a.MedicalCenterId == DevelopmentDataSeeder.DefaultMedicalCenterId)
            .ToListAsync();

        availability.Should().HaveCount(3);
        availability.Select(a => a.DayOfWeek).Should().BeEquivalentTo(["Monday", "Wednesday", "Friday"]);
        availability.Should().OnlyContain(a => a.IsAvailable == true);
        availability.Should().OnlyContain(a => a.StartTime!.Value.Hour == 9);
        availability.Should().OnlyContain(a => a.EndTime!.Value.Hour == 17);
    }

    [Fact]
    public async Task SeedAsync_SeedsAvailabilityForEachDoctorMedicalCenter()
    {
        await using var provider = CreateProvider();
        await using (var setupScope = provider.CreateAsyncScope())
        {
            var setupContext = setupScope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
            setupContext.MedicalCenter.Add(new MedicalCenter
            {
                Id = 9,
                StreetAddress = "900 Doctor Lane",
                City = "Boston",
                State = "MA",
                Zip = "02108"
            });
            setupContext.Doctors.Add(new Doctor
            {
                Id = "doctor-uat-1",
                Name = "Dr. UAT",
                Email = "doctor-uat-1@example.com",
                MedicalCenterId = 9
            });
            await setupContext.SaveChangesAsync();
        }

        await DevelopmentDataSeeder.SeedAsync(provider);

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        var doctorCenterAvailability = await context.MedicalCenterDoctorAvailability
            .Where(a => a.MedicalCenterId == 9)
            .ToListAsync();

        doctorCenterAvailability.Should().HaveCount(3);
        doctorCenterAvailability.Should().OnlyContain(a => a.IsAvailable == true);
    }

    [Fact]
    public async Task SeedAsync_MedicalCenterAndAvailabilityAreIdempotent()
    {
        await using var provider = CreateProvider();

        await DevelopmentDataSeeder.SeedAsync(provider);
        await DevelopmentDataSeeder.SeedAsync(provider);

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        (await context.MedicalCenter.CountAsync()).Should().Be(1);
        (await context.MedicalCenterDoctorAvailability.CountAsync()).Should().Be(3);
    }

    [Fact]
    public async Task SeedAsync_CreatesAdminDoctorAndPatientUsersWithRoles()
    {
        await using var provider = CreateProvider();
        await DevelopmentDataSeeder.SeedAsync(provider);

        await using var scope = provider.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var admin = await userManager.FindByEmailAsync(DevelopmentDataSeeder.AdminEmail);
        admin.Should().NotBeNull();
        (await userManager.IsInRoleAsync(admin!, "admin")).Should().BeTrue();

        var doctorSmith = await userManager.FindByEmailAsync(DevelopmentDataSeeder.DoctorSmithEmail);
        doctorSmith.Should().NotBeNull();
        (await userManager.IsInRoleAsync(doctorSmith!, "doctor")).Should().BeTrue();

        var doctorJones = await userManager.FindByEmailAsync(DevelopmentDataSeeder.DoctorJonesEmail);
        doctorJones.Should().NotBeNull();
        (await userManager.IsInRoleAsync(doctorJones!, "doctor")).Should().BeTrue();

        var patientAlice = await userManager.FindByEmailAsync(DevelopmentDataSeeder.PatientAliceEmail);
        patientAlice.Should().NotBeNull();
        (await userManager.IsInRoleAsync(patientAlice!, "user")).Should().BeTrue();

        var patientBob = await userManager.FindByEmailAsync(DevelopmentDataSeeder.PatientBobEmail);
        patientBob.Should().NotBeNull();
        (await userManager.IsInRoleAsync(patientBob!, "user")).Should().BeTrue();
    }

    [Fact]
    public async Task SeedAsync_SeededUsersCanAuthenticateWithKnownPassword()
    {
        await using var provider = CreateProvider();
        await DevelopmentDataSeeder.SeedAsync(provider);

        await using var scope = provider.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var admin = await userManager.FindByEmailAsync(DevelopmentDataSeeder.AdminEmail);
        var passwordValid = await userManager.CheckPasswordAsync(admin!, DevelopmentDataSeeder.SeedPassword);
        passwordValid.Should().BeTrue();
    }

    [Fact]
    public async Task SeedAsync_CreatesDoctorProfilesWithRelatedRecords()
    {
        await using var provider = CreateProvider();
        await DevelopmentDataSeeder.SeedAsync(provider);

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        var doctors = await context.Doctors.ToListAsync();
        doctors.Should().HaveCount(2);

        foreach (var doctor in doctors)
        {
            doctor.Name.Should().NotBeNullOrWhiteSpace();
            doctor.ProfessionalStatement.Should().NotBeNullOrWhiteSpace();
            doctor.PracticingFrom.Should().NotBeNull();
            doctor.MedicalCenterId.Should().Be(DevelopmentDataSeeder.DefaultMedicalCenterId);

            var specializationCount = await context.DoctorSpecialization
                .CountAsync(ds => ds.DoctorId == doctor.Id);
            specializationCount.Should().BeGreaterThanOrEqualTo(1);

            var qualificationCount = await context.DoctorQualifications
                .CountAsync(q => q.DoctorId == doctor.Id);
            qualificationCount.Should().BeGreaterThanOrEqualTo(1);

            var affiliationCount = await context.HospitalAffiliation
                .CountAsync(h => h.DoctorId == doctor.Id);
            affiliationCount.Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Fact]
    public async Task SeedAsync_CreatesPatientsWithProfileFields()
    {
        await using var provider = CreateProvider();
        await DevelopmentDataSeeder.SeedAsync(provider);

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        var patients = await context.Patients.ToListAsync();
        patients.Should().HaveCount(2);
        patients.Should().OnlyContain(p => !string.IsNullOrWhiteSpace(p.Name));
        patients.Should().OnlyContain(p => !string.IsNullOrWhiteSpace(((AppUser)p).Address));
    }

    [Fact]
    public async Task SeedAsync_CreatesAppointmentsAcrossMultipleStatuses()
    {
        await using var provider = CreateProvider();
        await DevelopmentDataSeeder.SeedAsync(provider);

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        var appointments = await context.Appointments
            .Include(a => a.AppointmentStatus)
            .ToListAsync();

        appointments.Should().HaveCountGreaterThanOrEqualTo(5);
        appointments.Select(a => a.AppointmentStatus!.Status).Distinct().Count().Should().BeGreaterThanOrEqualTo(2);
        appointments.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.DoctorId));
        appointments.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.PatientId));
        appointments.Should().OnlyContain(a => a.MedicalCenterId == DevelopmentDataSeeder.DefaultMedicalCenterId);
        appointments.Should().OnlyContain(a => a.Amount == 30.00m);
    }

    [Fact]
    public async Task SeedAsync_IsIdempotentForUsersDoctorsPatientsAndAppointments()
    {
        await using var provider = CreateProvider();

        await DevelopmentDataSeeder.SeedAsync(provider);

        await using (var scope = provider.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            var userCount = await userManager.Users.CountAsync();
            var doctorCount = await context.Doctors.CountAsync();
            var patientCount = await context.Patients.CountAsync();
            var appointmentCount = await context.Appointments.CountAsync();

            await DevelopmentDataSeeder.SeedAsync(provider);

            (await userManager.Users.CountAsync()).Should().Be(userCount);
            (await context.Doctors.CountAsync()).Should().Be(doctorCount);
            (await context.Patients.CountAsync()).Should().Be(patientCount);
            (await context.Appointments.CountAsync()).Should().Be(appointmentCount);
        }
    }
}
