namespace AngularApi.Contracts.DTO
{
    public class ReviewDTO
    {
        public int Id { get; set; }
        public string? PatientId { get; set; }
        public string? DoctorId { get; set; }
        public bool? IsReviewAnonymous { get; set; }
        public int? WaitTimeRating { get; set; }
        public int? BedsideMannerRating { get; set; }
        public int? OverallRating { get; set; }
        public string? Review { get; set; }
        public bool? IsDoctorRecommended { get; set; }
        public DateTime? ReviewDate { get; set; }
    }
}
