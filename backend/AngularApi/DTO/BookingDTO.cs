namespace AngularApi.DTO
{
    public class BookingDTO
    {
        public int AppointmentId { get; set; }
        public string? PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
        public DateTime? AppointmentTakenDate { get; set; }
        public string? AppointmentStatus { get; set; }
    }
}
