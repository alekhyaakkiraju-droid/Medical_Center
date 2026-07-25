using AngularApi.Models;
using AngularApi.Services.impelementation;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AngularApi.Tests.Services;

public class JwtServiceTests
{
    [Fact]
    public async Task GenerateJwtTokenResultAsync_UsesRolesFromUserManager()
    {
        var user = new AppUser
        {
            Id = "user-1",
            Email = "user@example.com",
            UserName = "user@example.com"
        };

        var userManager = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);
        userManager.Setup(manager => manager.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "admin", "user" });

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "integration-test-secret-key-32chars!",
                ["Jwt:ValidIssuer"] = "test-issuer",
                ["Jwt:ValidAudience"] = "test-audience"
            })
            .Build();

        var service = new JwtService(userManager.Object, configuration);

        var result = await service.GenerateJwtTokenResultAsync(user);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Token);
        var roles = jwt.Claims.Where(claim => claim.Type == ClaimTypes.Role).Select(claim => claim.Value);

        roles.Should().Contain(new[] { "admin", "user" });
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_ReturnsTokenString()
    {
        var user = new AppUser
        {
            Id = "user-2",
            Email = "doctor@example.com",
            UserName = "doctor@example.com"
        };

        var userManager = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);
        userManager.Setup(manager => manager.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "doctor" });

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "integration-test-secret-key-32chars!",
                ["Jwt:ValidIssuer"] = "test-issuer",
                ["Jwt:ValidAudience"] = "test-audience"
            })
            .Build();

        var service = new JwtService(userManager.Object, configuration);

        var token = await service.GenerateJwtTokenAsync(user);

        token.Should().NotBeNullOrWhiteSpace();
    }
}
