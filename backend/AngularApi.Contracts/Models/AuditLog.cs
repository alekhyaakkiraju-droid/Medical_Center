namespace AngularApi.Contracts.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string Actor { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
    }
}
