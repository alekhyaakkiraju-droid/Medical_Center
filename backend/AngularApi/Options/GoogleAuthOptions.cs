namespace AngularApi.Options;

public class GoogleAuthOptions
{
    public const string SectionName = "GoogleAuth";

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId)
        && !string.IsNullOrWhiteSpace(ClientSecret)
        && !string.Equals(ClientId, "placeholder", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(ClientSecret, "placeholder", StringComparison.OrdinalIgnoreCase);
}
