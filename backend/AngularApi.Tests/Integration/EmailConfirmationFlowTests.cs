using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using AngularApi.Contracts.Models;
using AngularApi.Tests.Infrastructure;
using AngularApi.Tests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Integration;

public class EmailConfirmationFlowTests : IClassFixture<MailHogWebApplicationFactory>
{
    private readonly MailHogWebApplicationFactory _factory;

    public EmailConfirmationFlowTests(MailHogWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RegisterUser_SendsConfirmationEmailToMailHog_AndConfirmEmailSetsEmailConfirmed()
    {
        if (!await MailHogTestHelper.IsAvailableAsync())
        {
            return;
        }

        await MailHogTestHelper.ClearMessagesAsync();

        var registerDto = RegisterUserFixtures.Valid();
        var client = await CreateClientForIpAsync($"203.0.113.{Random.Shared.Next(1, 200)}");

        var registerResponse = await client.PostAsJsonAsync("/api/Account/register/user", registerDto);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var message = await MailHogTestHelper.WaitForMessageToAsync(registerDto.Email!);
        message.Should().NotBeNull();

        var body = message!.Content.Body;
        body.Should().Contain(registerDto.UserName!);
        body.Should().Contain("/auth/confirm-email?userId=");

        var confirmationUrl = ExtractConfirmationUrl(body);
        confirmationUrl.Should().NotBeNullOrWhiteSpace();

        var confirmUri = new Uri(confirmationUrl!);
        var query = QueryHelpers.ParseQuery(confirmUri.Query);
        var userId = query["userId"].ToString();
        var token = query["token"].ToString();
        userId.Should().NotBeNullOrWhiteSpace();
        token.Should().NotBeNullOrWhiteSpace();

        var confirmResponse = await client.GetAsync(
            $"/api/Account/confirm-email?userId={Uri.EscapeDataString(userId!)}&token={Uri.EscapeDataString(token!)}");
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var scope = _factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByEmailAsync(registerDto.Email!);
        user.Should().NotBeNull();
        user!.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmEmail_WithInvalidToken_ReturnsBadRequest()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var registerDto = RegisterUserFixtures.Valid();
        var user = new Patient
        {
            UserName = registerDto.UserName,
            Email = registerDto.Email,
            EmailConfirmed = false,
        };
        (await userManager.CreateAsync(user, registerDto.Password!)).Succeeded.Should().BeTrue();

        var client = await CreateClientForIpAsync($"203.0.113.{Random.Shared.Next(201, 254)}");
        var confirmResponse = await client.GetAsync(
            $"/api/Account/confirm-email?userId={Uri.EscapeDataString(user.Id)}&token=invalid-token");
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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

    private static string? ExtractConfirmationUrl(string emailBody)
    {
        var match = Regex.Match(
            emailBody,
            @"https?://[^\s""']+/auth/confirm-email\?userId=[^""'\s<]+",
            RegexOptions.IgnoreCase);
        return match.Success ? match.Value : null;
    }
}
