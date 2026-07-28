using AngularApi.Contracts.Services;
using AngularApi.Contracts.Models;
using AngularApi.Options;
using AngularApi.Services.impelementation;
using AngularApi.Contracts.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;

namespace AngularApi.Tests.Services;

public class AuthCookieServiceTests
{
    [Fact]
    public async Task IssueAuthCookiesAsync_ProductionEnvironment_SetsSecureCookieOptions()
    {
        var httpContext = new DefaultHttpContext();
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(accessor => accessor.HttpContext).Returns(httpContext);

        var jwtService = new Mock<IJwtService>();
        jwtService.Setup(service => service.GenerateJwtTokenResultAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(new JwtTokenResult("jwt-token", "jwt-id", DateTime.UtcNow.AddHours(1)));

        var refreshTokenService = new Mock<IRefreshTokenService>();
        refreshTokenService.Setup(service => service.CreateRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync("refresh-token");

        var userManager = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(env => env.EnvironmentName).Returns(Environments.Production);

        var service = new AuthCookieService(
            httpContextAccessor.Object,
            jwtService.Object,
            refreshTokenService.Object,
            userManager.Object,
            Microsoft.Extensions.Options.Options.Create(new AuthCookieOptions()),
            environment.Object);

        await service.IssueAuthCookiesAsync(new AppUser
        {
            Id = "user-1",
            Email = "user@example.com",
            UserName = "user@example.com",
        });

        httpContext.Response.Headers.SetCookie.ToString().Should().Contain("secure");
    }
}
