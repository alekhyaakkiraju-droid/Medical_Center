using AngularApi.Models;
using AngularApi.Services;
using AngularApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AngularApi.Services.impelementation
{
    public class JwtService : IJwtService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public JwtService(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public string GenerateJwtToken(AppUser user) => GenerateJwtTokenResult(user).Token;

        public JwtTokenResult GenerateJwtTokenResult(AppUser user)
        {
            ValidateUser(user);
            var claims = GetClaimsForUser(user);
            var signingCredentials = GetSigningCredentials();
            var jti = Guid.NewGuid().ToString();
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, jti));

            var expiresUtc = DateTime.UtcNow.AddDays(1);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:ValidIssuer"],
                audience: _configuration["Jwt:ValidAudience"],
                claims: claims,
                expires: expiresUtc,
                signingCredentials: signingCredentials);

            return new JwtTokenResult(new JwtSecurityTokenHandler().WriteToken(token), jti, expiresUtc);
        }

        public JwtTokenResult? ReadToken(string token, bool validateLifetime = true)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var handler = new JwtSecurityTokenHandler();
            try
            {
                handler.ValidateToken(token, BuildValidationParameters(validateLifetime), out var validatedToken);
                var jwt = (JwtSecurityToken)validatedToken;
                var jti = jwt.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrEmpty(jti))
                {
                    return null;
                }

                return new JwtTokenResult(token, jti, jwt.ValidTo);
            }
            catch
            {
                return null;
            }
        }

        private void ValidateUser(AppUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrEmpty(user.Id))
            {
                throw new ArgumentNullException(nameof(user.Id), "User Id cannot be null or empty");
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                throw new ArgumentNullException(nameof(user.Email), "User Email cannot be null or empty");
            }

            if (string.IsNullOrEmpty(user.UserName))
            {
                throw new ArgumentNullException(nameof(user.UserName), "User Name cannot be null or empty");
            }
        }

        private List<Claim> GetClaimsForUser(AppUser user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.UserName),
            };

            var roles = _userManager.GetRolesAsync(user).Result;
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        private SigningCredentials GetSigningCredentials()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        private TokenValidationParameters BuildValidationParameters(bool validateLifetime) => new()
        {
            ValidateIssuer = true,
            ValidIssuer = _configuration["Jwt:ValidIssuer"],
            ValidateAudience = true,
            ValidAudience = _configuration["Jwt:ValidAudience"],
            ValidateLifetime = validateLifetime,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!)),
        };
    }
}
