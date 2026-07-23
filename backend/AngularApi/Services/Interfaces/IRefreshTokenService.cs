namespace AngularApi.Services.Interfaces;

public interface IRefreshTokenService
{
    Task<string> CreateRefreshTokenAsync(string userId, string jwtId, CancellationToken cancellationToken = default);

    Task<bool> ValidateAndRevokeAsync(string userId, string jwtId, string refreshToken, CancellationToken cancellationToken = default);
}
