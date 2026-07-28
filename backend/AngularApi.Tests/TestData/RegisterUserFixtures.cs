using AngularApi.Contracts.DTO;

namespace AngularApi.Tests.TestData;

public static class RegisterUserFixtures
{
    public static RegisterUserDTO Valid(string? suffix = null)
    {
        var unique = suffix ?? new string(Enumerable.Range(0, 8)
            .Select(_ => (char)('A' + Random.Shared.Next(26)))
            .ToArray());
        return new RegisterUserDTO
        {
            UserName = $"TestUser{unique}",
            Email = $"testuser{unique.ToLowerInvariant()}@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!",
        };
    }
}
