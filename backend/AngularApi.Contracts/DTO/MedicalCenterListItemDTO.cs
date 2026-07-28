namespace AngularApi.Contracts.DTO
{
    public class MedicalCenterListItemDTO
    {
        public int Id { get; set; }
        public int? HospitalAffiliationId { get; set; }
        public int? TimeSlotPerClientInMin { get; set; }
        public decimal? FirstConsultationFee { get; set; }
        public decimal? FollowupConsultationFee { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
    }
}
