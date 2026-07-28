namespace AngularApi.Options;

public class BaaFeatureFlags
{
    public const string SectionName = "BaaFeatureFlags";

    public bool SmtpBaaExecuted { get; set; } = true;

    public bool AwsBaaExecuted { get; set; } = true;
}
