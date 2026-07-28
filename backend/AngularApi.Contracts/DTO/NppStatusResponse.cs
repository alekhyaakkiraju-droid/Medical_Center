namespace AngularApi.Contracts.DTO;

public class NppStatusResponse
{
    public bool Acknowledged { get; set; }

    public DateTime? AcknowledgedAt { get; set; }

    public string Version { get; set; } = string.Empty;
}
