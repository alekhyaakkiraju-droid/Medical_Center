using AngularApi.Contracts.Models;
using Microsoft.AspNetCore.Identity;
using AngularApi.Services;

namespace AngularApi.Tests.TestData;

public static class PasswordResetUserFixtures
{
    public const string Email = "passwordreset.user@example.com";
    public const string Password = "OriginalPassword123!";
    public const string NewPassword = "UpdatedPassword456!";
    public const string UserName = "PasswordResetUser";

    public static async Task<AppUser> SeedAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole>? roleManager = null)
    {
        if (roleManager != null)
        {
            await roleManager.EnsureRolesCreatedAsync();
        }

        var existing = await userManager.FindByEmailAsync(Email);
        if (existing != null)
        {
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(existing);
            await userManager.ResetPasswordAsync(existing, resetToken, Password);
            return existing;
        }

        var user = new Patient
        {
            UserName = UserName,
            Email = Email,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to seed password reset test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(user, "user");
        return user;
    }
}
