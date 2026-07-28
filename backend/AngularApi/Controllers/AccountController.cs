using AngularApi.Contracts.Services;
﻿using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;
using AngularApi.Options;
using AngularApi.Services;
using AngularApi.Contracts.Services.Interfaces;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using Response = AngularApi.Services.Response;

namespace AngularApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {

        /// <summary>
        /// Authenticates a user and issues an HttpOnly auth cookie.
        /// Example request body: { "email": "user@example.com", "password": "YourSecurePassword123!" }
        /// </summary>
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _Configuration;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IJwtService _jwtService;
        private readonly IGoogleService _googleService;
        private readonly EmailTemplateService _emailTemplateService;
        private readonly IAuthCookieService _authCookieService;
        private readonly IAntiforgery _antiforgery;
        private readonly AuthCookieOptions _authCookieOptions;
        private readonly GoogleAuthOptions _googleAuthOptions;
        private readonly IAuditService _auditService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<AppUser> userManager,
            IConfiguration Configuration,
            IEmailService emailService,
            EmailTemplateService emailTemplateService,
            IJwtService jwtService,
            IGoogleService googleService,
            IAuthCookieService authCookieService,
            IAntiforgery antiforgery,
            IOptions<AuthCookieOptions> authCookieOptions,
            IOptions<GoogleAuthOptions> googleAuthOptions,
            IAuditService auditService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _Configuration = Configuration;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
            _jwtService = jwtService;
            _googleService = googleService;
            _authCookieService = authCookieService;
            _antiforgery = antiforgery;
            _authCookieOptions = authCookieOptions.Value;
            _googleAuthOptions = googleAuthOptions.Value;
            _auditService = auditService;
            _logger = logger;
        }

        //public AccountController(UserManager<AppUser> userManager, IConfiguration Configuration, IEmailService emailService,
        //   IJwtService jwtService, IGoogleService googleService)
        //{
        //    _userManager = userManager;
        //    _Configuration = Configuration;
        //    _emailService = emailService;
        //    _jwtService = jwtService;
        //    _googleService = googleService;
        //    // this._signInManager = _signInManager;

        //}
        [AllowAnonymous]
        [EnableRateLimiting(AuthRateLimitingExtensions.RegisterPolicy)]
        [HttpPost("register/user")]
        public async Task<IActionResult> Register(RegisterUserDTO registerUser)
        {
            if (ModelState.IsValid)
            {
                Patient appUser = new Patient();
                appUser.UserName = registerUser.UserName;
                appUser.Email = registerUser.Email;

                IdentityResult result = await _userManager.CreateAsync(appUser, registerUser.Password);

                if (result.Succeeded)
                {

                    await _userManager.AddToRoleAsync(appUser, "user");
                    /// return Ok(new { message = "Account created successfully with role." });
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                    var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account",
                        new { userId = appUser.Id, token }, Request.Scheme);

                    var confirmtionLinkForFront =
                        $"{_authCookieOptions.FrontendBaseUrl}/auth/confirm-email?userId={appUser.Id}&token={WebUtility.UrlEncode(token)}";

                    var emailBody = _emailTemplateService.GetConfirmationEmail(appUser.UserName, confirmtionLinkForFront);
                    var message = new Message(new[] { appUser.Email }, "Confirm Your Email", emailBody);

                    try
                    {
                        await _emailService.SendEmailAsync(message);
                        return Ok(new { message = "Account created successfully. Please check your email to confirm your account." });
                    }
                    catch (Exception ex)
                    {
                        // Handle email sending failure
                        return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to send email. Please try again later." });
                    }
                }
                return BadRequest(result.Errors.FirstOrDefault().Description.ToString());
            }
            return BadRequest(ModelState);
        }


        [Authorize(Policy = "AdminPolicy")]
        [EnableRateLimiting(AuthRateLimitingExtensions.RegisterPolicy)]
        [HttpPost("Register/admin")]
        public async Task<IActionResult> RegisterWithAdmin(RegisterUserDTO registerUser)
        {
            if (ModelState.IsValid)
            {
                AppUser appUser = new AppUser();
                appUser.UserName = registerUser.UserName;
                appUser.Email = registerUser.Email;

                // appUser.PasswordHash = registerUser.Password;
                IdentityResult result = await _userManager.CreateAsync(appUser, registerUser.Password);
                if (result.Succeeded)
                {
                    // var role = registerUser.Role ?? "user"; // Default role to "user"
                    await _userManager.AddToRoleAsync(appUser, "admin");
                    return Ok(new { message = "Account created successfully with role." });
                }
                return BadRequest(result.Errors.FirstOrDefault().Description.ToString());
            }
            return BadRequest(ModelState);
        }

        [Authorize(Policy = "AdminPolicy")]
        [EnableRateLimiting(AuthRateLimitingExtensions.RegisterPolicy)]
        [HttpPost("Register/doctor")]
        public async Task<IActionResult> RegisterWithDoctor(RegisterUserDTO registerUser)
        {
            if (ModelState.IsValid)
            {
                Doctor appUser = new Doctor();
                appUser.UserName = registerUser.UserName;
                appUser.Email = registerUser.Email;

                IdentityResult result = await _userManager.CreateAsync(appUser, registerUser.Password);
                if (result.Succeeded)
                {
                    // var role = registerUser.Role ?? "user"; // Default role to "user"
                    await _userManager.AddToRoleAsync(appUser, "doctor");
                    return Ok(new { message = "Account created successfully with role." });
                }
                return BadRequest(result.Errors.FirstOrDefault().Description.ToString());
            }
            return BadRequest(ModelState);
        }


        [AllowAnonymous]
        [EnableRateLimiting(AuthRateLimitingExtensions.LoginPolicy)]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LogInUserDTO logInUser)
        {
            if (ModelState.IsValid)
            {
                var found = await _userManager.FindByEmailAsync(logInUser.Email);
                if (found != null)
                {
                    AppUser appUser = new AppUser();
                    appUser.Email = logInUser.Email;

                    var checkpass = await _userManager.CheckPasswordAsync(found, logInUser.Password);
                    if (checkpass)
                    {
                        var cookieResult = await _authCookieService.IssueAuthCookiesAsync(found);
                        await _auditService.RecordAuthEventAsync("LoginSuccess", found.Email, true);
                        return Ok(new
                        {
                            expiration = cookieResult.ExpirationUtc
                        });
                    }
                }
                await _auditService.RecordAuthEventAsync("LoginFailure", logInUser.Email, false);
                return Unauthorized();
            }
            return BadRequest(ModelState);
        }


        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpGet("LoginWithGoogle")]
        public IActionResult LoginWithGoogle()
        {
            if (!_googleAuthOptions.IsConfigured)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    error = "OAuthNotConfigured",
                    message = "Google OAuth is not configured for this environment.",
                });
            }

            var properties = _googleService.GetGoogleLoginProperties(Url.Action(nameof(GoogleLoginCallback)));
            return Challenge(properties, "Google");
        }


        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpGet("GoogleLoginCallback")]
        public async Task<IActionResult> GoogleLoginCallback()
        {
            if (!_googleAuthOptions.IsConfigured)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    error = "OAuthNotConfigured",
                    message = "Google OAuth is not configured for this environment.",
                });
            }

            try
            {
                var user = await _googleService.GoogleLoginCallbackAsync();
                await _authCookieService.IssueAuthCookiesAsync(user);
                return Redirect(_authCookieOptions.FrontendLoginSuccessUrl);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("External login failed.");
            }
            catch
            {
                return BadRequest("An error occurred during Google login.");
            }
        }


        [AllowAnonymous]
        [EnableRateLimiting(AuthRateLimitingExtensions.ForgotPasswordPolicy)]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO forgotPasswordDto)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
                if (user == null)
                {
                    return Ok(new { message = "If an account with that email exists, a reset link has been sent." });
                }
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebUtility.UrlEncode(resetToken);

                var resetLink =
                    $"{_authCookieOptions.FrontendBaseUrl}/auth/reset-password?token={encodedToken}&email={WebUtility.UrlEncode(user.Email)}";
                var message = new Message(new[] { user.Email }, "Forgot Password Link", resetLink);

                try
                {
                    await _emailService.SendEmailAsync(message);
                    await _auditService.RecordAuthEventAsync("PasswordResetRequested", user.Email, true);
                    return Ok(new Response("Success", $"Password reset link sent to {user.Email}. Please check your email."));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send password reset email");
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response("Error", "Failed to send email, please try again later."));
                }

            }

            return BadRequest(ModelState);
        }

        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpGet("reset-password")]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return BadRequest(new { Status = "Error", Message = "Invalid password reset link." });
            }

            // Return success response for valid tokens
            //return Ok(new { Status = "Success", Message = "Password reset link is valid.", Token = token, Email = email }); 
            return Redirect(
                $"{_authCookieOptions.FrontendBaseUrl}/auth/reset-password?token={token}&email={email}");
        }



        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDto)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
                if (user == null)
                {
                    return BadRequest(new { message = "Invalid request." });
                }

                var decodedToken = WebUtility.UrlDecode(resetPasswordDto.Token);
                var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);

                if (result.Succeeded)
                {
                    await _auditService.RecordAuthEventAsync("PasswordResetCompleted", user.Email, true);
                    return Ok(new { message = "Password has been reset successfully." });
                }

                _logger.LogWarning("Password reset failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest(result.Errors.FirstOrDefault()?.Description);
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Changes the authenticated user's password.
        /// Example request body: { "currentPassword": "OldPassword123!", "newPassword": "NewPassword123!" }
        /// </summary>
        /// 

        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpGet("antiforgery-token")]
        public IActionResult GetAntiforgeryToken()
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            return Ok(new { token = tokens.RequestToken });
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            if (!Request.Cookies.TryGetValue(_authCookieOptions.AuthCookieName, out var jwtToken)
                || !Request.Cookies.TryGetValue(_authCookieOptions.RefreshCookieName, out var refreshToken))
            {
                return Unauthorized();
            }

            try
            {
                var cookieResult = await _authCookieService.RefreshAuthCookiesAsync(jwtToken, refreshToken);
                return Ok(new { expiration = cookieResult.ExpirationUtc });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }


        [Authorize(Policy = "UserPolicy")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok("Password changed successfully");
        }


        [Authorize(Policy = "UserPolicy")]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.Address = model.Address;
            user.PhoneNumber = model.PhoneNumber;


            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok("Profile updated successfully");
        }

        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded) return BadRequest("Email confirmation failed");

            return Ok(new { Message = "Email confirmed successfully." });
        }

        [AllowAnonymous]
        [HttpPost("resend-email-confirmation")]
        public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return NotFound("User not found");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, token }, Request.Scheme);

            // Assume a method for sending email exists
            //await SendEmailAsync(user.Email, "Confirm your email", confirmationLink);

            return Ok("Email confirmation link sent");
        }


        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                userId = user.Id,
                email = user.Email,
                userName = user.UserName,
                roles,
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var actor = User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? "anonymous";
            _authCookieService.ClearAuthCookies();
            await _auditService.RecordAuthEventAsync("Logout", actor, true);
            return Ok(new { message = "Logged out successfully" });
        }

        [Authorize(Policy = "UserPolicy")]
        [HttpGet("user-details")]
        public async Task<IActionResult> GetUserDetails()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            //var user = await _userService.GetCurrentUserAsync();
            if (user == null) return NotFound("User not found");

            return Ok(new
            {
                user.Email,
                user.UserName,
                user.Address,
                user.PhoneNumber
            });
        }


        [Authorize(Policy = "UserPolicy")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok("Account deleted successfully");
        }


    }

}
