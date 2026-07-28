using AngularApi.Contracts.Enums;
using AngularApi.Models;
using AngularApi.Contracts.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Services;

/// <summary>
/// Seeds UAT reference data in Development when entities are missing.
/// Each seed step is idempotent so partial or repeated runs do not duplicate records.
/// </summary>
public static class DevelopmentDataSeeder
{
    /// <summary>
    /// Matches <see cref="Options.AppointmentSettings.DefaultCenterId"/>.
    /// </summary>
    public const int DefaultMedicalCenterId = 2;

    public const string SeedPassword = "UatSeed123!";

    public const string AdminEmail = "admin@uat.careshift.local";
    public const string DoctorSmithEmail = "dr.smith@uat.careshift.local";
    public const string DoctorJonesEmail = "dr.jones@uat.careshift.local";
    public const string PatientAliceEmail = "patient.alice@uat.careshift.local";
    public const string PatientBobEmail = "patient.bob@uat.careshift.local";
    public const string PatientCarolEmail = "patient.carol@uat.careshift.local";

    private static readonly string[] AvailabilityDays = ["Monday", "Wednesday", "Friday"];

    /// <summary>
    /// Canonical reference statuses. Ids match <see cref="AppointmentStatusEnum"/> values
    /// so services can assign <c>AppointmentStatusId = (int)AppointmentStatusEnum.*</c>.
    /// </summary>
    internal static readonly (int Id, AppointmentStatusEnum Status)[] CanonicalAppointmentStatuses =
    [
        ((int)AppointmentStatusEnum.Active, AppointmentStatusEnum.Active),
        ((int)AppointmentStatusEnum.Complete, AppointmentStatusEnum.Complete),
        ((int)AppointmentStatusEnum.Canceled, AppointmentStatusEnum.Canceled),
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await roleManager.EnsureRolesCreatedAsync();

        await SeedSpecializationsAsync(context);
        await SeedAppointmentStatusesAsync(context);

        var medicalCenterId = await SeedMedicalCentersAsync(context);

        await SeedAdminUserAsync(userManager);
        var doctors = await SeedDoctorsAsync(context, userManager, medicalCenterId);
        var patients = await SeedPatientsAsync(context, userManager);

        await SeedDoctorAvailabilityAsync(context, medicalCenterId);
        await SeedAppointmentsAsync(context, doctors, patients, medicalCenterId);
        await RepairSeedAssetPathsAsync(context);
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
            SpecializationImage = "images/services/service-one.jpg",
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
            SpecializationImage = "images/services/service-two.jpg",
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
            SpecializationImage = "images/services/service-three.jpg",
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

    private static async Task SeedAppointmentStatusesAsync(MedicalCenterDbContext context)
    {
        var toInsert = new List<AppointmentStatus>();

        foreach (var (id, status) in CanonicalAppointmentStatuses)
        {
            if (await context.AppointmentStatus.AnyAsync(s => s.Status == status))
            {
                continue;
            }

            toInsert.Add(new AppointmentStatus { Id = id, Status = status });
        }

        if (toInsert.Count == 0)
        {
            return;
        }

        await InsertAppointmentStatusesAsync(context, toInsert);
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

    private static async Task SeedAdminUserAsync(UserManager<AppUser> userManager)
    {
        if (await userManager.FindByEmailAsync(AdminEmail) != null)
        {
            return;
        }

        var admin = new AppUser
        {
            UserName = AdminEmail,
            Email = AdminEmail,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(admin, SeedPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to seed admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(admin, "admin");
    }

    private static async Task<IReadOnlyList<Doctor>> SeedDoctorsAsync(
        MedicalCenterDbContext context,
        UserManager<AppUser> userManager,
        int medicalCenterId)
    {
        var specializations = await context.Specializations
            .OrderBy(s => s.Id)
            .ToListAsync();

        var doctorDefinitions = new[]
        {
            new
            {
                Email = DoctorSmithEmail,
                Name = "Dr. Alice Smith",
                Image = "images/team/doctor-1.jpg",
                ProfessionalStatement = "Board-certified cardiologist focused on preventive heart care.",
                PracticingFrom = new DateTime(2010, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                SpecializationName = "Cardiology",
                QualificationName = "MD Cardiology",
                InstituteName = "Johns Hopkins School of Medicine",
                ProcurementYear = new DateTime(2008, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                HospitalName = "City Heart Institute",
                HospitalCity = "Boston",
                HospitalCountry = "USA",
                HospitalStartDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            },
            new
            {
                Email = DoctorJonesEmail,
                Name = "Dr. Robert Jones",
                Image = "images/team/doctor-2.jpg",
                ProfessionalStatement = "Orthopedic surgeon specializing in sports medicine and joint repair.",
                PracticingFrom = new DateTime(2008, 3, 15, 0, 0, 0, DateTimeKind.Utc),
                SpecializationName = "Orthopedics",
                QualificationName = "MD Orthopedics",
                InstituteName = "Stanford University School of Medicine",
                ProcurementYear = new DateTime(2006, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                HospitalName = "Regional Orthopedic Center",
                HospitalCity = "Chicago",
                HospitalCountry = "USA",
                HospitalStartDate = new DateTime(2010, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            }
        };

        var doctors = new List<Doctor>();

        foreach (var definition in doctorDefinitions)
        {
            var existingDoctor = await context.Doctors.FirstOrDefaultAsync(d => d.Email == definition.Email);
            if (existingDoctor != null)
            {
                doctors.Add(existingDoctor);
                continue;
            }

            var doctor = new Doctor
            {
                UserName = definition.Email,
                Email = definition.Email,
                EmailConfirmed = true,
                Name = definition.Name,
                Image = definition.Image,
                ProfessionalStatement = definition.ProfessionalStatement,
                PracticingFrom = definition.PracticingFrom,
                MedicalCenterId = medicalCenterId,
            };

            var result = await userManager.CreateAsync(doctor, SeedPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to seed doctor {definition.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await userManager.AddToRoleAsync(doctor, "doctor");

            var specialization = specializations.First(s => s.SpecializationName == definition.SpecializationName);
            context.DoctorSpecialization.Add(new DoctorSpecialization
            {
                DoctorId = doctor.Id,
                SpecializationId = specialization.Id,
            });

            context.DoctorQualifications.Add(new DoctorQualification
            {
                DoctorId = doctor.Id,
                QualificationName = definition.QualificationName,
                InstituteName = definition.InstituteName,
                ProcurementYear = definition.ProcurementYear,
            });

            context.HospitalAffiliation.Add(new HospitalAffiliation
            {
                DoctorId = doctor.Id,
                HospitalName = definition.HospitalName,
                City = definition.HospitalCity,
                Country = definition.HospitalCountry,
                StartDate = definition.HospitalStartDate,
            });

            await context.SaveChangesAsync();
            doctors.Add(doctor);
        }

        return doctors;
    }

    private static async Task<IReadOnlyList<Patient>> SeedPatientsAsync(
        MedicalCenterDbContext context,
        UserManager<AppUser> userManager)
    {
        var patientDefinitions = new[]
        {
            new
            {
                Email = PatientAliceEmail,
                Name = "Alice Nguyen",
                Address = "123 Maple Street, Springfield, IL 62701",
                Image = "images/patients/patients-1.jpg",
            },
            new
            {
                Email = PatientBobEmail,
                Name = "Bob Martinez",
                Address = "456 Oak Avenue, Portland, OR 97201",
                Image = "images/patients/patients-2.jpg",
            },
            new
            {
                Email = PatientCarolEmail,
                Name = "Carol Chen",
                Address = "789 Pine Road, Austin, TX 78701",
                Image = "images/patients/patients-3.jpg",
            }
        };

        var patients = new List<Patient>();

        foreach (var definition in patientDefinitions)
        {
            var existingPatient = await context.Patients.FirstOrDefaultAsync(p => p.Email == definition.Email);
            if (existingPatient != null)
            {
                patients.Add(existingPatient);
                continue;
            }

            var patient = new Patient
            {
                UserName = definition.Email,
                Email = definition.Email,
                EmailConfirmed = true,
                Name = definition.Name,
                Image = definition.Image,
            };
            ((AppUser)patient).Address = definition.Address;

            var result = await userManager.CreateAsync(patient, SeedPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to seed patient {definition.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await userManager.AddToRoleAsync(patient, "user");
            patients.Add(patient);
        }

        return patients;
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

    private sealed record AppointmentSeedDefinition(
        int DoctorIndex,
        int PatientIndex,
        int DateOffsetDays,
        int StartHour,
        AppointmentStatusEnum Status,
        string PaymentStatus,
        bool IsFollowUp,
        int? DurationMinutes = null);

    private static async Task SeedAppointmentsAsync(
        MedicalCenterDbContext context,
        IReadOnlyList<Doctor> doctors,
        IReadOnlyList<Patient> patients,
        int medicalCenterId)
    {
        if (doctors.Count < 2 || patients.Count < 2)
        {
            return;
        }

        if (await context.Appointments.AnyAsync(a => a.DoctorId == doctors[0].Id))
        {
            return;
        }

        var medicalCenter = await context.MedicalCenter.FindAsync(medicalCenterId);
        var firstConsultationFee = medicalCenter?.FirstConsultationFee ?? 50.00m;
        var followupConsultationFee = medicalCenter?.FollowupConsultationFee ?? 30.00m;
        var today = DateTime.UtcNow.Date;

        var definitions = new AppointmentSeedDefinition[]
        {
            new(0, 0, 0, 9, AppointmentStatusEnum.Active, "Pending", false),
            new(0, 0, 0, 14, AppointmentStatusEnum.Active, "Paid", true),
            new(0, 1, 0, 11, AppointmentStatusEnum.Active, "Pending", false),
            new(0, 1, 7, 10, AppointmentStatusEnum.Active, "Pending", false),
            new(0, 2, 14, 15, AppointmentStatusEnum.Active, "Pending", false),
            new(0, 2, -1, 16, AppointmentStatusEnum.Complete, "Paid", true),
            new(1, 0, 0, 10, AppointmentStatusEnum.Active, "Pending", false),
            new(1, 0, 3, 13, AppointmentStatusEnum.Active, "Pending", false),
            new(1, 1, -2, 9, AppointmentStatusEnum.Canceled, "Refunded", false),
            new(1, 1, 21, 11, AppointmentStatusEnum.Active, "Pending", false),
            new(1, 2, 0, 15, AppointmentStatusEnum.Active, "Pending", false),
            new(1, 2, -7, 12, AppointmentStatusEnum.Complete, "Paid", true),
            new(0, 0, -14, 10, AppointmentStatusEnum.Complete, "Paid", false),
            new(0, 1, -3, 13, AppointmentStatusEnum.Complete, "Paid", true),
            new(0, 2, 5, 9, AppointmentStatusEnum.Active, "Pending", false),
            new(1, 0, -5, 14, AppointmentStatusEnum.Canceled, "Refunded", false),
            new(1, 1, 10, 16, AppointmentStatusEnum.Active, "Pending", false),
            new(1, 2, 28, 10, AppointmentStatusEnum.Active, "Pending", false),
        };

        var appointments = definitions
            .Select(definition =>
            {
                var doctor = doctors[Math.Min(definition.DoctorIndex, doctors.Count - 1)];
                var patient = patients[Math.Min(definition.PatientIndex, patients.Count - 1)];
                var appointmentDate = today.AddDays(definition.DateOffsetDays);
                var startTime = appointmentDate.AddHours(definition.StartHour);
                var fee = definition.IsFollowUp ? followupConsultationFee : firstConsultationFee;

                return new Appointment
                {
                    DoctorId = doctor.Id,
                    PatientId = patient.Id,
                    MedicalCenterId = medicalCenterId,
                    DoctorName = doctor.Name,
                    Name = patient.Name,
                    Email = patient.Email,
                    Phone = $"555-01{definition.DoctorIndex}{definition.PatientIndex}{Math.Abs(definition.DateOffsetDays) % 10}",
                    AppointmentStatusId = (int)definition.Status,
                    AppointmentTakenDate = appointmentDate,
                    ProbableStartTime = startTime,
                    ActualEndTime = definition.Status == AppointmentStatusEnum.Complete
                        ? startTime.AddMinutes(definition.DurationMinutes ?? 30)
                        : null,
                    Amount = fee,
                    PaymentStatus = definition.PaymentStatus,
                };
            })
            .ToArray();

        context.Appointments.AddRange(appointments);
        await context.SaveChangesAsync();
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

    private static async Task InsertAppointmentStatusesAsync(
        MedicalCenterDbContext context,
        IReadOnlyList<AppointmentStatus> statuses)
    {
        if (context.Database.IsSqlServer())
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                foreach (var status in statuses)
                {
                    await context.Database.ExecuteSqlInterpolatedAsync(
                        $"""
                        SET IDENTITY_INSERT [AppointmentStatus] ON;
                        INSERT INTO [AppointmentStatus] ([Id], [Status]) VALUES ({status.Id}, {(int)status.Status!});
                        SET IDENTITY_INSERT [AppointmentStatus] OFF;
                        """);
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return;
        }

        foreach (var status in statuses)
        {
            var entity = new AppointmentStatus { Status = status.Status };
            var entry = context.AppointmentStatus.Add(entity);
            entry.Property(s => s.Id).CurrentValue = status.Id;
            entry.Property(s => s.Id).IsTemporary = false;
            await context.SaveChangesAsync();
        }
    }

    private static async Task InsertMedicalCenterAsync(MedicalCenterDbContext context, MedicalCenter center)
    {
        if (context.Database.IsSqlServer())
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [MedicalCenter] ON");
                var entry = context.MedicalCenter.Add(center);
                entry.Property(c => c.Id).IsTemporary = false;
                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [MedicalCenter] OFF");
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return;
        }

        context.MedicalCenter.Add(center);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates canonical UAT asset paths when seed data was created with legacy placeholders.
    /// Safe to run on every Development startup.
    /// </summary>
    private static async Task RepairSeedAssetPathsAsync(MedicalCenterDbContext context)
    {
        var specializationImages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Orthopedics"] = "images/services/service-one.jpg",
            ["Cardiology"] = "images/services/service-two.jpg",
            ["Pediatrics"] = "images/services/service-three.jpg",
        };

        foreach (var specialization in await context.Specializations.ToListAsync())
        {
            if (specializationImages.TryGetValue(specialization.SpecializationName, out var image)
                && specialization.SpecializationImage != image)
            {
                specialization.SpecializationImage = image;
            }
        }

        var doctorImages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [DoctorSmithEmail] = "images/team/doctor-1.jpg",
            [DoctorJonesEmail] = "images/team/doctor-2.jpg",
        };

        foreach (var doctor in await context.Doctors.ToListAsync())
        {
            if (doctor.Email != null
                && doctorImages.TryGetValue(doctor.Email, out var image)
                && doctor.Image != image)
            {
                doctor.Image = image;
            }
        }

        var patientImages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [PatientAliceEmail] = "images/patients/patients-1.jpg",
            [PatientBobEmail] = "images/patients/patients-2.jpg",
        };

        foreach (var patient in await context.Patients.ToListAsync())
        {
            if (patient.Email != null
                && patientImages.TryGetValue(patient.Email, out var image)
                && patient.Image != image)
            {
                patient.Image = image;
            }
        }

        await context.SaveChangesAsync();
    }
}
