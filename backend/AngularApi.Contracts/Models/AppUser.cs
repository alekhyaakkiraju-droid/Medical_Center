using Microsoft.AspNetCore.Identity;

namespace AngularApi.Contracts.Models
{
    public class AppUser :IdentityUser
    {
        public string? Address { get; set; }
    }
}
