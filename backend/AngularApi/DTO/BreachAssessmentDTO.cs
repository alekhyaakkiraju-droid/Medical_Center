namespace AngularApi.DTO
{
    public class BreachAssessmentDTO
    {
        public string Description { get; set; } = string.Empty;
        public IList<string> AffectedEntityTypes { get; set; } = new List<string>();
        public DateTime DiscoveryDate { get; set; }
        public string SeverityLevel { get; set; } = string.Empty;
        public IList<string> AffectedIndividualEmails { get; set; } = new List<string>();
    }
}
