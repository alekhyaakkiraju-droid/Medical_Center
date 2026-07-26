using AngularApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Services;

/// <summary>
/// Seeds minimal demo data in Development when the database is empty.
/// </summary>
public static class DevelopmentDataSeeder
{
    /// <summary>
    /// Matches <see cref="Options.AppointmentSettings.DefaultCenterId"/>.
    /// </summary>
    public const int DefaultMedicalCenterId = 2;

    private static readonly string[] AvailabilityDays = ["Monday", "Wednesday", "Friday"];

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

        await SeedSpecializationsAsync(context);
        var medicalCenterId = await SeedMedicalCentersAsync(context);
        await SeedDoctorAvailabilityAsync(context, medicalCenterId);
    }

    private static async Task SeedSpecializationsAsync(MedicalCenterDbContext context)
    {
        if (await context.Specializations.AnyAsync())
        {
            return;
        }

        var orthopedics = new Specialization
        {
            SpecializationName = "Orthopedics",
            SpecializationImage = "images/resource/1.png",
            Description = "Bone, joint, and muscle care.",
            IsActive = true,
            Services =
            [
                new Service { Name = "Joint Replacement", Description = "Advanced joint replacement procedures." },
                new Service { Name = "Sports Injury", Description = "Treatment for sports-related injuries." }
            ]
        };

        var cardiology = new Specialization
        {
            SpecializationName = "Cardiology",
            SpecializationImage = "images/resource/2.png",
            Description = "Heart and cardiovascular care.",
            IsActive = true,
            Services =
            [
                new Service { Name = "ECG Screening", Description = "Routine cardiac screening." },
                new Service { Name = "Hypertension Care", Description = "Blood pressure management." }
            ]
        };

        var pediatrics = new Specialization
        {
            SpecializationName = "Pediatrics",
            SpecializationImage = "images/resource/3.png",
            Description = "Healthcare for children and adolescents.",
            IsActive = true,
            Services =
            [
                new Service { Name = "Well-Child Visits", Description = "Preventive pediatric checkups." },
                new Service { Name = "Immunizations", Description = "Vaccination programs." }
            ]
        };

        context.Specializations.AddRange(orthopedics, cardiology, pediatrics);
        await context.SaveChangesAsync();
    }

    internal static async Task<int> SeedMedicalCentersAsync(MedicalCenterDbContext context)
    {
        var existingDefaultCenter = await context.MedicalCenter.FindAsync(DefaultMedicalCenterId);
        if (existingDefaultCenter != null)
        {
            return existingDefaultCenter.Id;
        }

        if (await context.MedicalCenter.AnyAsync())
        {
            return (await context.MedicalCenter.OrderBy(c => c.Id).FirstAsync()).Id;
        }

        var center = new MedicalCenter
        {
            Id = DefaultMedicalCenterId,
            TimeSlotPerClientInMin = 30,
            FirstConsultationFee = 50.00m,
            FollowupConsultationFee = 30.00m,
            StreetAddress = "450 CareShift Medical Plaza",
            City = "Springfield",
            State = "IL",
            Zip = "62701"
        };

        await InsertMedicalCenterAsync(context, center);
        return center.Id;
    }

    internal static async Task SeedDoctorAvailabilityAsync(MedicalCenterDbContext context, int defaultMedicalCenterId)
    {
        var doctorCenterIds = await context.Doctors
            .Where(d => d.MedicalCenterId != null)
            .Select(d => d.MedicalCenterId!.Value)
            .Distinct()
            .ToListAsync();

        var centerIds = doctorCenterIds.Count > 0
            ? doctorCenterIds
            : [defaultMedicalCenterId];

        foreach (var centerId in centerIds.Distinct())
        {
            if (await context.MedicalCenterDoctorAvailability.AnyAsync(a => a.MedicalCenterId == centerId))
            {
                continue;
            }

            if (!await context.MedicalCenter.AnyAsync(c => c.Id == centerId))
            {
                continue;
            }

            var slots = AvailabilityDays.Select(day => CreateAvailabilitySlot(centerId, day));
            context.MedicalCenterDoctorAvailability.AddRange(slots);
            await context.SaveChangesAsync();
        }
    }

    private static MedicalCenterDoctorAvailability CreateAvailabilitySlot(int medicalCenterId, string dayOfWeek) =>
        new()
        {
            MedicalCenterId = medicalCenterId,
            DayOfWeek = dayOfWeek,
            StartTime = DateTime.Today.AddHours(9),
            EndTime = DateTime.Today.AddHours(17),
            IsAvailable = true
        };

    private static async Task InsertMedicalCenterAsync(MedicalCenterDbContext context, MedicalCenter center)
    {
        if (context.Database.IsSqlServer())
        {
            await context.Database.OpenConnectionAsync();
            try
            {
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [MedicalCenter] ON");
                context.MedicalCenter.Add(center);
                await context.SaveChangesAsync();
            }
            finally
            {
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [MedicalCenter] OFF");
                await context.Database.CloseConnectionAsync();
            }

            return;
        }

        context.MedicalCenter.Add(center);
        await context.SaveChangesAsync();
    }
}
