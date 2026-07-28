using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace AngularApi.Tests.Infrastructure;

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

public static class MailHogTestHelper
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

    public static async Task<int> GetMessageCountForAsync(string recipientEmail)
    {
        var messages = await GetMessagesAsync();
        return messages.Count(m =>
            m.To.Any(t => string.Equals(t.Email, recipientEmail, StringComparison.OrdinalIgnoreCase)));
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

public sealed class MailHogMessagesResponse
{
    public List<MailHogMessage> Items { get; set; } = [];
}

public sealed class MailHogMessage
{
    public MailHogMessageContent Content { get; set; } = new();
    public List<MailHogRecipient> To { get; set; } = [];
}

public sealed class MailHogMessageContent
{
    public string Body { get; set; } = string.Empty;
}

public sealed class MailHogRecipient
{
    public string Mailbox { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Email => $"{Mailbox}@{Domain}";
}
