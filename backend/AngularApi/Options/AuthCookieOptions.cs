namespace AngularApi.Options;

public class AuthCookieOptions
{
    public const string SectionName = "Jwt";

    public string AuthCookieName { get; set; } = "MedCenter.Auth";

    public string RefreshCookieName { get; set; } = "MedCenter.Refresh";

    public string CookiePath { get; set; } = "/api";

    public string FrontendLoginSuccessUrl { get; set; } = string.Empty;
}
