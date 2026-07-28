namespace AngularApi.Contracts.Models
{
    public class Doctor : AppUser, IAuditableEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? Name { get; set; } = string.Empty;
        public string? Image { get; set; } = string.Empty;
        public string ?ProfessionalStatement { get; set; } = string.Empty;
        public DateTime?PracticingFrom { get; set; }
        public int? MedicalCenterId { get; set; }

        public MedicalCenter? MedicalCenter { get; set; }
        public ICollection<DoctorSpecialization>? DoctorSpecializations { get; set; } = new List<DoctorSpecialization>();
        public ICollection<DoctorQualification>? Qualifications { get; set; } = new List<DoctorQualification>();
        public ICollection<HospitalAffiliation>? HospitalAffiliations { get; set; } = new List<HospitalAffiliation>();
        public ICollection<PatientReview>? PatientReviews { get; set; } = new List<PatientReview>();
    }
}
