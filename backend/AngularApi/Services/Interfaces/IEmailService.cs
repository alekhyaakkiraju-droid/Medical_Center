namespace AngularApi.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(Message message);
    }
}
