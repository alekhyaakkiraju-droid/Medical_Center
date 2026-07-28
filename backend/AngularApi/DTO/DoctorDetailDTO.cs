namespace AngularApi.DTO
{
    public record DoctorQualificationSummary(
        string? QualificationName,
        string? InstituteName,
        DateTime? ProcurementYear);

    public record HospitalAffiliationSummary(
        string? HospitalName,
        string? City,
        string? Country,
        DateTime? StartDate,
        DateTime? EndDate);

    public class DoctorDetailDTO
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Image { get; set; }
        public string? ProfessionalStatement { get; set; }
        public DateTime? PracticingFrom { get; set; }
        public List<string>? Specializations { get; set; }
        public List<DoctorQualificationSummary>? Qualifications { get; set; }
        public List<HospitalAffiliationSummary>? HospitalAffiliations { get; set; }
        public double AverageRating { get; set; }
    }
}
