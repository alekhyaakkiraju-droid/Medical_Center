using AngularApi.Models;
using AngularApi.Services;

namespace AngularApi.Services.Interfaces;

public interface IAuthCookieService
{
    Task<AuthCookieIssueResult> IssueAuthCookiesAsync(AppUser user, CancellationToken cancellationToken = default);

    Task<AuthCookieIssueResult> RefreshAuthCookiesAsync(string jwtToken, string refreshToken, CancellationToken cancellationToken = default);

    void ClearAuthCookies();
}
