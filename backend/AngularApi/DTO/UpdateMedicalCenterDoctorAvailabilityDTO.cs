namespace AngularApi.DTO
{
    public class UpdateMedicalCenterDoctorAvailabilityDTO
    {
        public int MedicalCenterId { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool? IsAvailable { get; set; }
        public string? ReasonOfUnavailability { get; set; }
    }
}
