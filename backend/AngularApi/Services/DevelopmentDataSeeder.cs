using AngularApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Services;

/// <summary>
/// Seeds minimal demo data in Development when the database is empty.
/// </summary>
public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();

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
}
