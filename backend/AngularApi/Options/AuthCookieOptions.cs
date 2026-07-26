namespace AngularApi.Options;

public class AuthCookieOptions
{
    public const string SectionName = "Jwt";

    public string AuthCookieName { get; set; } = "MedCenter.Auth";

    public string RefreshCookieName { get; set; } = "MedCenter.Refresh";

    public string CookiePath { get; set; } = "/api";

    private string _frontendBaseUrl = "http://localhost:8081";

    public string FrontendBaseUrl
    {
        get => _frontendBaseUrl;
        set => _frontendBaseUrl = string.IsNullOrWhiteSpace(value)
            ? _frontendBaseUrl
            : value.TrimEnd('/');
    }

    private string? _frontendLoginSuccessUrl;

    public string FrontendLoginSuccessUrl
    {
        get => _frontendLoginSuccessUrl ?? $"{FrontendBaseUrl}/auth/login-success";
        set => _frontendLoginSuccessUrl = string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
