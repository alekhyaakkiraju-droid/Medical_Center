namespace AngularApi.Contracts.Services;

public record JwtTokenResult(string Token, string JwtId, DateTime ExpiresUtc);
