namespace AngularApi.Contracts.DTO
{
    public class BreachAnomalyDTO
    {
        public string AnomalyType { get; set; } = string.Empty;
        public string Actor { get; set; } = string.Empty;
        public int EventCount { get; set; }
        public DateTime WindowStart { get; set; }
        public DateTime WindowEnd { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
