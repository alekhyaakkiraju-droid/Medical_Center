using AngularApi.Models;
using AngularApi.Services;

namespace AngularApi.Services.Interfaces;

public interface IJwtService
{
    Task<string> GenerateJwtTokenAsync(AppUser user);

    Task<JwtTokenResult> GenerateJwtTokenResultAsync(AppUser user);

    JwtTokenResult? ReadToken(string token, bool validateLifetime = true);
}
