using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AngularApi.Tests.Infrastructure;

public static class TestJwtFactory
{
    public static string CreateToken(IConfiguration configuration, params string[] roles)
        => CreateTokenForUser(configuration, "integration-test-user", roles);

    public static string CreateTokenForUser(IConfiguration configuration, string userId, params string[] roles)
    {
        var claims = BuildClaims(userId, roles);
        return CreateToken(configuration, claims, DateTime.UtcNow.AddHours(1));
    }

    public static string CreateExpiredTokenForUser(IConfiguration configuration, string userId, params string[] roles)
    {
        var claims = BuildClaims(userId, roles);
        return CreateToken(configuration, claims, DateTime.UtcNow.AddMinutes(-10));
    }

    private static List<Claim> BuildClaims(string userId, string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userId),
            new(ClaimTypes.Email, $"{userId}@example.com"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }

    private static string CreateToken(IConfiguration configuration, List<Claim> claims, DateTime expiresUtc)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:ValidIssuer"],
            audience: configuration["Jwt:ValidAudience"],
            claims: claims,
            expires: expiresUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
