namespace AngularApi.Contracts.Services.Interfaces
{
    public interface IRecaptchaService
    {
        Task<bool> ValidateTokenAsync(string token);
    }
}
