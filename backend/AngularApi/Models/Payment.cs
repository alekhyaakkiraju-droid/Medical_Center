namespace AngularApi.Models
{
    public class Payment : IAuditableEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public int? AppointmentId { get; set; }
        public decimal? Amount { get; set; }
        public string? PaymentMethod { get; set; } 
        public string? PaymentStatus { get; set; } 
        public DateTime? PaymentDate { get; set; }
        public Appointment? Appointment { get; set; } 
    }

}
