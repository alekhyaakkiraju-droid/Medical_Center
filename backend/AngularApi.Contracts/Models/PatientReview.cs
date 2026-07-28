namespace AngularApi.Contracts.Models
{
    public class PatientReview : IAuditableEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? PatientId { get; set; }
        public string? DoctorId { get; set; }
        public bool? IsReviewAnonymous { get; set; }
        public int? WaitTimeRating { get; set; }
        public int? BedsideMannerRating { get; set; }
        public int? OverallRating { get; set; }
        public string? Review { get; set; } = string.Empty;
        public bool? IsDoctorRecommended { get; set; }
        public DateTime? ReviewDate { get; set; }

        public Patient? Patient { get; set; } = null!;
        public Doctor? Doctor { get; set; } = null!;
    }

}
