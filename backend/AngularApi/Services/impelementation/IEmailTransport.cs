using AngularApi.Options;
using MimeKit;

namespace AngularApi.Services.impelementation;

public interface IEmailTransport
{
    Task SendAsync(
        MimeMessage message,
        SmtpSettings smtpSettings,
        string username,
        string password,
        CancellationToken cancellationToken = default);
}
