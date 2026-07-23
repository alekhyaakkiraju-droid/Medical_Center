using AngularApi.Models;
using AngularApi.Services;

namespace AngularApi.Services.Interfaces;

public interface IJwtService
{
    string GenerateJwtToken(AppUser user);

    JwtTokenResult GenerateJwtTokenResult(AppUser user);

    JwtTokenResult? ReadToken(string token, bool validateLifetime = true);
}
