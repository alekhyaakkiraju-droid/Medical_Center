using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace AngularApi.Models
{
    public class MedicalCenterDbContext : IdentityDbContext<AppUser>
    {
        public MedicalCenterDbContext() { }
        public MedicalCenterDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AppointmentStatus> AppointmentStatus { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorSpecialization> DoctorSpecialization { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<DoctorQualification> DoctorQualifications { get; set; }
        public DbSet<HospitalAffiliation> HospitalAffiliation { get; set; }        
        public DbSet<MedicalCenterDoctorAvailability> MedicalCenterDoctorAvailability { get; set; }       
        public DbSet<MedicalCenter> MedicalCenter { get; set; }
        public DbSet<PatientReview> PatientReviews { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Doctor>()
                .ToTable("Doctors");

            modelBuilder.Entity<Patient>()
              .ToTable("Patients");

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLogs");
                entity.HasKey(log => log.Id);
            });
        }

        public override int SaveChanges()
        {
            EnforceAuditLogAppendOnly();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            EnforceAuditLogAppendOnly();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void EnforceAuditLogAppendOnly()
        {
            foreach (var entry in ChangeTracker.Entries<AuditLog>())
            {
                if (entry.State is EntityState.Modified or EntityState.Deleted)
                {
                    throw new InvalidOperationException("AuditLog records are append-only.");
                }

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.Timestamp = DateTime.UtcNow;
                }
            }
        }

    }
}
