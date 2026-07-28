using AngularApi.Contracts.Models;

namespace AngularApi.Contracts.Services.Interfaces
{
    public interface IUserService
    {
        Task<AppUser> GetCurrentUserAsync();
    }
}
