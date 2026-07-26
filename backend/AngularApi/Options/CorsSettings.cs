namespace AngularApi.Options;

public class CorsSettings
{
    public const string SectionName = "CorsSettings";

    public static readonly string[] DefaultOrigins =
    [
        "http://localhost:4200",
        "http://localhost:8081"
    ];

    public string[] AllowedOrigins { get; set; } = [];
}
