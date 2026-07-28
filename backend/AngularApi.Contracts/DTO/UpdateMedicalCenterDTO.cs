namespace AngularApi.Contracts.DTO
{
    public class UpdateMedicalCenterDTO
    {
        public int? HospitalAffiliationId { get; set; }
        public int? TimeSlotPerClientInMin { get; set; }
        public decimal? FirstConsultationFee { get; set; }
        public decimal? FollowupConsultationFee { get; set; }
        public string StreetAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
    }
}
