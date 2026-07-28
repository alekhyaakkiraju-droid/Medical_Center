using AngularApi.Contracts.Models;
using Microsoft.AspNetCore.Authentication;

namespace AngularApi.Contracts.Services.Interfaces
{
    public interface IGoogleService
    {
        AuthenticationProperties GetGoogleLoginProperties(string redirectUri);
        Task<AppUser> GoogleLoginCallbackAsync();
    }
}
