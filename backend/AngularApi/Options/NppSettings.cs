namespace AngularApi.Options;

public class NppSettings
{
    public const string SectionName = "NppSettings";

    public string CurrentVersion { get; set; } = "1.0";

    public string ContentFilePath { get; set; } = "wwwroot/legal/npp.md";
}
