namespace AngularApi.Models;

public class RefreshToken
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string TokenHash { get; set; } = string.Empty;

    public string JwtId { get; set; } = string.Empty;

    public DateTime ExpiresUtc { get; set; }

    public DateTime CreatedUtc { get; set; }

    public bool IsRevoked { get; set; }
}
