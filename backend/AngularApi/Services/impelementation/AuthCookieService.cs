using AngularApi.Contracts.Services;
using AngularApi.Contracts.Models;
using AngularApi.Options;
using AngularApi.Services;
using AngularApi.Contracts.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AngularApi.Services.impelementation;

public class AuthCookieService : IAuthCookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly UserManager<AppUser> _userManager;
    private readonly AuthCookieOptions _options;
    private readonly IWebHostEnvironment _environment;

    public AuthCookieService(
        IHttpContextAccessor httpContextAccessor,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService,
        UserManager<AppUser> userManager,
        IOptions<AuthCookieOptions> options,
        IWebHostEnvironment environment)
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _userManager = userManager;
        _options = options.Value;
        _environment = environment;
    }

    public async Task<AuthCookieIssueResult> IssueAuthCookiesAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        var jwt = await _jwtService.GenerateJwtTokenResultAsync(user);
        var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user.Id, jwt.JwtId, cancellationToken);
        SetAuthCookies(jwt.Token, refreshToken, jwt.ExpiresUtc);
        return new AuthCookieIssueResult(jwt.ExpiresUtc);
    }

    public async Task<AuthCookieIssueResult> RefreshAuthCookiesAsync(
        string jwtToken,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var parsedToken = _jwtService.ReadToken(jwtToken, validateLifetime: false);
        if (parsedToken == null)
        {
            throw new UnauthorizedAccessException("Invalid auth cookie.");
        }

        var userId = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
            .ReadJwtToken(jwtToken)
            .Claims.FirstOrDefault(claim => claim.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("Invalid auth cookie.");
        }

        var isValidRefreshToken = await _refreshTokenService.ValidateAndRevokeAsync(
            userId,
            parsedToken.JwtId,
            refreshToken,
            cancellationToken);

        if (!isValidRefreshToken)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new UnauthorizedAccessException("User not found.");

        return await IssueAuthCookiesAsync(user, cancellationToken);
    }

    public void ClearAuthCookies()
    {
        var response = GetResponse();
        response.Cookies.Delete(_options.AuthCookieName, new CookieOptions { Path = _options.CookiePath });
        response.Cookies.Delete(_options.RefreshCookieName, new CookieOptions { Path = _options.CookiePath });
    }

    private void SetAuthCookies(string jwtToken, string refreshToken, DateTime jwtExpirationUtc)
    {
        var response = GetResponse();
        var refreshExpiration = DateTime.UtcNow.AddDays(7);

        response.Cookies.Append(_options.AuthCookieName, jwtToken, CreateCookieOptions(jwtExpirationUtc));
        response.Cookies.Append(_options.RefreshCookieName, refreshToken, CreateCookieOptions(refreshExpiration));
    }

    private CookieOptions CreateCookieOptions(DateTime expiresUtc) => new()
    {
        HttpOnly = true,
        Secure = _environment.IsProduction(),
        SameSite = SameSiteMode.Strict,
        Path = _options.CookiePath,
        Expires = new DateTimeOffset(expiresUtc),
    };

    private HttpResponse GetResponse()
        => _httpContextAccessor.HttpContext?.Response
            ?? throw new InvalidOperationException("No active HTTP context is available.");
}
