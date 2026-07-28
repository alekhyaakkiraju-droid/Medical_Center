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

public sealed class MailHogWebApplicationFactory : MedicalCenterWebApplicationFactory
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EmailSettings:EmailUsername"] = "test@example.com",
                ["EmailSettings:EmailPassword"] = "test-password",
                ["SmtpSettings:Host"] = Environment.GetEnvironmentVariable("MAILHOG_SMTP_HOST") ?? "localhost",
                ["SmtpSettings:Port"] = Environment.GetEnvironmentVariable("MAILHOG_SMTP_PORT") ?? "1025",
                ["SmtpSettings:UseTls"] = "false",
            });
        });
    }
}

internal static class MailHogTestHelper
{
    private static readonly HttpClient Client = new();
    private static readonly string MailHogBaseUrl =
        Environment.GetEnvironmentVariable("MAILHOG_WEB_URL") ?? "http://localhost:8025";

    public static async Task<bool> IsAvailableAsync()
    {
        try
        {
            using var response = await Client.GetAsync($"{MailHogBaseUrl}/");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public static Task ClearMessagesAsync() =>
        Client.DeleteAsync($"{MailHogBaseUrl}/api/v1/messages");

    public static async Task<MailHogMessage?> WaitForMessageToAsync(string recipientEmail, int timeoutSeconds = 10)
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
        while (DateTime.UtcNow < deadline)
        {
            var messages = await GetMessagesAsync();
            var match = messages.FirstOrDefault(m =>
                m.To.Any(t => string.Equals(t.Email, recipientEmail, StringComparison.OrdinalIgnoreCase)));
            if (match != null)
            {
                return match;
            }

            await Task.Delay(500);
        }

        return null;
    }

    private static async Task<IReadOnlyList<MailHogMessage>> GetMessagesAsync()
    {
        using var response = await Client.GetAsync($"{MailHogBaseUrl}/api/v2/messages");
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        var payload = await JsonSerializer.DeserializeAsync<MailHogMessagesResponse>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return payload?.Items ?? [];
    }
}

internal sealed class MailHogMessagesResponse
{
    public List<MailHogMessage> Items { get; set; } = [];
}

internal sealed class MailHogMessage
{
    public MailHogMessageContent Content { get; set; } = new();
    public List<MailHogRecipient> To { get; set; } = [];
}

internal sealed class MailHogMessageContent
{
    public string Body { get; set; } = string.Empty;
}

internal sealed class MailHogMessageBody
{
    public string Body { get; set; } = string.Empty;
}

internal sealed class MailHogRecipient
{
    public string Mailbox { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Email => $"{Mailbox}@{Domain}";
}
