using AngularApi.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AngularApi.Services.impelementation;

public sealed class MailKitEmailTransport : IEmailTransport
{
    public async Task SendAsync(
        MimeMessage message,
        SmtpSettings smtpSettings,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        using var smtpClient = new SmtpClient();
        var secureSocketOptions = smtpSettings.UseTls
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.None;

        await smtpClient.ConnectAsync(smtpSettings.Host, smtpSettings.Port, secureSocketOptions, cancellationToken);
        await smtpClient.AuthenticateAsync(username, password, cancellationToken);
        await smtpClient.SendAsync(message, cancellationToken);
        await smtpClient.DisconnectAsync(true, cancellationToken);
    }
}
