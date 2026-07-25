namespace AngularApi.Services;

public record JwtTokenResult(string Token, string JwtId, DateTime ExpiresUtc);
