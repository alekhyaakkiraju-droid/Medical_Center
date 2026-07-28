namespace AngularApi.Contracts.DTO
{
    public class DoctorQualificationDTO
    {
        public int Id { get; set; }
        public string? DoctorId { get; set; }
        public string? QualificationName { get; set; }
        public string? InstituteName { get; set; }
        public DateTime? ProcurementYear { get; set; }
    }
}
