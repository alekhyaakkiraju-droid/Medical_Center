using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AngularApi.Infrastructure;

public static class JwtSecretStartupValidation
{
    public const string ConfigurationKey = "Jwt:Secret";

    public static void Validate(IConfiguration configuration, ILogger logger)
    {
        if (!string.IsNullOrWhiteSpace(configuration[ConfigurationKey]))
        {
            return;
        }

        logger.LogWarning(
            "Jwt:Secret is not configured. JWT authentication requires a signing key from " +
            "appsettings, Docker secrets (/run/secrets/jwt_secret), JWT_SECRET_FILE, or Jwt__Secret.");

        throw new InvalidOperationException(
            "Jwt:Secret is not configured. Provide it via appsettings (Jwt:Secret), " +
            "mount Docker secret /run/secrets/jwt_secret, set JWT_SECRET_FILE to a secret file path, " +
            "or configure the Jwt__Secret environment variable.");
    }
}
