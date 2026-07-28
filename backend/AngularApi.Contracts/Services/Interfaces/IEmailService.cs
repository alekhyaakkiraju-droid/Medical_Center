namespace AngularApi.Contracts.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(Message message);
    }
}
