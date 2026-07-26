using Microsoft.Extensions.Configuration;

namespace AngularApi.Infrastructure;

public sealed class DockerSecretConfigurationProvider : ConfigurationProvider
{
    internal const string SecretsDirectory = "/run/secrets";

    internal static readonly IReadOnlyDictionary<string, string> SecretFileMappings =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["jwt_secret"] = "Jwt:Secret",
            ["smtp_email_username"] = "EmailSettings:EmailUsername",
            ["smtp_email_password"] = "EmailSettings:EmailPassword",
            ["mssql_sa_password"] = "ConnectionStrings:SaPassword",
        };

    internal static readonly IReadOnlyDictionary<string, string> SecretFileEnvironmentMappings =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["JWT_SECRET_FILE"] = "Jwt:Secret",
            ["SMTP_EMAIL_USERNAME_FILE"] = "EmailSettings:EmailUsername",
            ["SMTP_EMAIL_PASSWORD_FILE"] = "EmailSettings:EmailPassword",
            ["MSSQL_SA_PASSWORD_FILE"] = "ConnectionStrings:SaPassword",
        };

    public override void Load()
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        if (Directory.Exists(SecretsDirectory))
        {
            foreach (var entry in LoadFromDirectory(SecretsDirectory))
            {
                data[entry.Key] = entry.Value;
            }
        }

        foreach (var mapping in SecretFileEnvironmentMappings)
        {
            var filePath = Environment.GetEnvironmentVariable(mapping.Key);
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                continue;
            }

            data[mapping.Value] = ReadSecretFile(filePath);
        }

        Data = data!;
    }

    public static Dictionary<string, string?> LoadFromDirectory(string secretsDirectory)
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var mapping in SecretFileMappings)
        {
            var secretPath = Path.Combine(secretsDirectory, mapping.Key);
            if (File.Exists(secretPath))
            {
                data[mapping.Value] = ReadSecretFile(secretPath);
            }
        }

        return data;
    }

    public static string ReadSecretFile(string path)
    {
        var contents = File.ReadAllText(path).Trim();
        if (string.IsNullOrEmpty(contents))
        {
            throw new InvalidOperationException($"Secret file '{path}' is empty.");
        }

        return contents;
    }
}
