using AngularApi.Models;
using AngularApi.Services.impelementation;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AngularApi.Tests.Services;

public class RefreshTokenServiceTests
{
    [Fact]
    public async Task ValidateAndRevokeAsync_ValidToken_ReturnsTrueAndRevokesToken()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new MedicalCenterDbContext(options);
        var configuration = new ConfigurationBuilder().Build();
        var service = new RefreshTokenService(context, configuration);

        var refreshToken = await service.CreateRefreshTokenAsync("user-1", "jwt-1");
        var isValid = await service.ValidateAndRevokeAsync("user-1", "jwt-1", refreshToken);

        isValid.Should().BeTrue();

        var replay = await service.ValidateAndRevokeAsync("user-1", "jwt-1", refreshToken);
        replay.Should().BeFalse();
    }
}
