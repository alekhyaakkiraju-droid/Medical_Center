namespace AngularApi.DTO
{
    public class MedicalCenterDoctorAvailabilityDTO
    {
        public int Id { get; set; }
        public int? MedicalCenterId { get; set; }
        public string? DayOfWeek { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool? IsAvailable { get; set; }
        public string? ReasonOfUnavailability { get; set; }
    }
}
