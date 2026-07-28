using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;
using AngularApi.Models;
using AngularApi.Tests.Infrastructure;
using AngularApi.Tests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Integration;

public class PasswordResetFlowTests : IClassFixture<MailHogWebApplicationFactory>
{
    private readonly MailHogWebApplicationFactory _factory;

    public PasswordResetFlowTests(MailHogWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ForgotPassword_ToResetPassword_AllowsLoginWithNewPassword()
    {
        if (!await MailHogTestHelper.IsAvailableAsync())
        {
            return;
        }

        await MailHogTestHelper.ClearMessagesAsync();

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await PasswordResetUserFixtures.SeedAsync(userManager, roleManager);
        }

        var clientIp = $"198.51.100.{Random.Shared.Next(1, 200)}";
        var client = await CreateClientForIpAsync(clientIp);
        var forgotResponse = await client.PostAsJsonAsync(
            "/api/Account/forgot-password",
            new ForgotPasswordDTO { Email = PasswordResetUserFixtures.Email });
        forgotResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var message = await MailHogTestHelper.WaitForMessageToAsync(PasswordResetUserFixtures.Email);
        message.Should().NotBeNull();

        var body = message!.Content.Body;
        body.Should().Contain("/auth/reset-password?token=");
        body.Should().Contain($"email={WebUtility.UrlEncode(PasswordResetUserFixtures.Email)}");

        var resetUrl = ExtractResetUrl(body);
        resetUrl.Should().NotBeNullOrWhiteSpace();

        var resetUri = new Uri(resetUrl!);
        var rawQuery = resetUri.Query.TrimStart('?');
        var token = ExtractQueryValue(rawQuery, "token");
        var email = WebUtility.UrlDecode(ExtractQueryValue(rawQuery, "email"));

        var resetResponse = await client.PostAsJsonAsync(
            "/api/Account/reset-password",
            new ResetPasswordDTO
            {
                Email = email,
                Token = token,
                NewPassword = PasswordResetUserFixtures.NewPassword,
            });
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await client.PostAsJsonAsync(
            "/api/Account/login",
            new LogInUserDTO
            {
                Email = PasswordResetUserFixtures.Email,
                Password = PasswordResetUserFixtures.NewPassword,
            });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var auditScope = _factory.Services.CreateAsyncScope();
        var context = auditScope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
        var auditEvents = await context.AuditLogs
            .Where(log => log.EntityId == PasswordResetUserFixtures.Email
                && (log.Action == "PasswordResetRequested" || log.Action == "PasswordResetCompleted"))
            .Select(log => log.Action)
            .ToListAsync();
        auditEvents.Should().Contain("PasswordResetRequested");
        auditEvents.Should().Contain("PasswordResetCompleted");
    }

    [Fact]
    public async Task ForgotPassword_NonExistentEmail_ReturnsOkWithoutSendingEmail()
    {
        if (!await MailHogTestHelper.IsAvailableAsync())
        {
            return;
        }

        await MailHogTestHelper.ClearMessagesAsync();
        var client = await CreateClientForIpAsync($"198.51.100.{Random.Shared.Next(201, 220)}");

        var response = await client.PostAsJsonAsync(
            "/api/Account/forgot-password",
            new ForgotPasswordDTO { Email = "missing-user@example.com" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await MailHogTestHelper.GetMessageCountForAsync("missing-user@example.com")).Should().Be(0);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ReturnsBadRequest()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        await PasswordResetUserFixtures.SeedAsync(userManager);

        var client = await CreateClientForIpAsync($"198.51.100.{Random.Shared.Next(221, 240)}");
        var response = await client.PostAsJsonAsync(
            "/api/Account/reset-password",
            new ResetPasswordDTO
            {
                Email = PasswordResetUserFixtures.Email,
                Token = "invalid-token",
                NewPassword = PasswordResetUserFixtures.NewPassword,
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ForgotPassword_ExceedingRateLimit_ReturnsTooManyRequests()
    {
        var client = await CreateClientForIpAsync("203.0.113.99");
        var payload = new ForgotPasswordDTO { Email = "rate-limit@example.com" };

        for (var attempt = 0; attempt < 3; attempt++)
        {
            var response = await client.PostAsJsonAsync("/api/Account/forgot-password", payload);
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }

        var limitedResponse = await client.PostAsJsonAsync("/api/Account/forgot-password", payload);
        limitedResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    private async Task<HttpClient> CreateClientForIpAsync(string ipAddress)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });
        client.DefaultRequestHeaders.Add("X-Test-Client-Ip", ipAddress);
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);
        return client;
    }

    private static string? ExtractResetUrl(string emailBody)
    {
        var match = Regex.Match(
            emailBody,
            @"https?://[^\s""']+/auth/reset-password\?token=[^""'\s<]+",
            RegexOptions.IgnoreCase);
        return match.Success ? match.Value : null;
    }

    private static string ExtractQueryValue(string rawQuery, string key)
    {
        foreach (var part in rawQuery.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var segments = part.Split('=', 2);
            if (segments.Length == 2 && string.Equals(segments[0], key, StringComparison.OrdinalIgnoreCase))
            {
                return segments[1];
            }
        }

        return string.Empty;
    }
}
