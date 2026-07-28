using AngularApi.Models;
using AngularApi.Contracts.Models;
using AngularApi.Contracts.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AngularApi.Services.impelementation;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly MedicalCenterDbContext _context;
    private readonly IConfiguration _configuration;

    public RefreshTokenService(MedicalCenterDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<string> CreateRefreshTokenAsync(string userId, string jwtId, CancellationToken cancellationToken = default)
    {
        var rawToken = GenerateSecureToken();
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            JwtId = jwtId,
            TokenHash = HashToken(rawToken),
            CreatedUtc = DateTime.UtcNow,
            ExpiresUtc = DateTime.UtcNow.AddDays(GetRefreshTokenLifetimeDays()),
            IsRevoked = false,
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return rawToken;
    }

    public async Task<bool> ValidateAndRevokeAsync(
        string userId,
        string jwtId,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(refreshToken);
        var storedToken = await _context.RefreshTokens
            .Where(token => token.UserId == userId
                && token.JwtId == jwtId
                && token.TokenHash == tokenHash
                && !token.IsRevoked
                && token.ExpiresUtc > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);

        if (storedToken == null)
        {
            return false;
        }

        storedToken.IsRevoked = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }

    private int GetRefreshTokenLifetimeDays()
        => int.TryParse(_configuration["Jwt:RefreshTokenLifetimeDays"], out var days) ? days : 7;
}
