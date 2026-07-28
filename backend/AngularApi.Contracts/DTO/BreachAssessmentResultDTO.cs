namespace AngularApi.Contracts.DTO
{
    public class BreachAssessmentResultDTO
    {
        public Guid AssessmentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int NotificationsSent { get; set; }
        public int NotificationsFailed { get; set; }
    }
}
