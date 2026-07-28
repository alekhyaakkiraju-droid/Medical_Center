using AngularApi.Contracts.Models;
using AngularApi.Contracts.Services;

namespace AngularApi.Contracts.Services.Interfaces;

public interface IJwtService
{
    Task<string> GenerateJwtTokenAsync(AppUser user);

    Task<JwtTokenResult> GenerateJwtTokenResultAsync(AppUser user);

    JwtTokenResult? ReadToken(string token, bool validateLifetime = true);
}
