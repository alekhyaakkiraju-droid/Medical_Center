using AngularApi.Infrastructure;
using AngularApi.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace AngularApi.Tests.Infrastructure;

public class DockerSecretConfigurationProviderTests
{
    private static readonly string FixturesDirectory = Path.Combine(
        AppContext.BaseDirectory,
        "Fixtures",
        "secrets");

    [Fact]
    public void Load_ReadsMappedSecretFiles_WhenSecretsDirectoryExists()
    {
        var tempSecretsDirectory = CreateTempSecretsDirectory(new Dictionary<string, string>
        {
            ["jwt_secret"] = "file-based-jwt-secret-value-32chars!",
            ["smtp_email_username"] = "smtp-user@example.com",
        });

        try
        {
            var data = DockerSecretConfigurationProvider.LoadFromDirectory(tempSecretsDirectory);

            data.TryGetValue("Jwt:Secret", out var jwtSecret).Should().BeTrue();
            jwtSecret.Should().Be("file-based-jwt-secret-value-32chars!");
            data.TryGetValue("EmailSettings:EmailUsername", out var smtpUser).Should().BeTrue();
            smtpUser.Should().Be("smtp-user@example.com");
        }
        finally
        {
            Directory.Delete(tempSecretsDirectory, recursive: true);
        }
    }

    [Fact]
    public void Load_FallsBackToExistingConfiguration_WhenSecretFilesAbsent()
    {
        var provider = new DockerSecretConfigurationProvider();
        provider.Load();

        provider.TryGet("Jwt:Secret", out _).Should().BeFalse();
    }

    [Fact]
    public void ReadSecretFile_TrimsWhitespace()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "  secret-value  \n");

            DockerSecretConfigurationProvider.ReadSecretFile(tempFile)
                .Should().Be("secret-value");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ReadSecretFile_Throws_WhenFileIsEmpty()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "   ");

            var act = () => DockerSecretConfigurationProvider.ReadSecretFile(tempFile);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*empty*");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Load_ReadsSecretFromEnvironmentFilePath()
    {
        var tempFile = Path.GetTempFileName();
        Environment.SetEnvironmentVariable("JWT_SECRET_FILE", tempFile);
        try
        {
            File.WriteAllText(tempFile, "env-file-jwt-secret-value-32chars!");

            var provider = new DockerSecretConfigurationProvider();
            provider.Load();

            provider.TryGet("Jwt:Secret", out var jwtSecret).Should().BeTrue();
            jwtSecret.Should().Be("env-file-jwt-secret-value-32chars!");
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_SECRET_FILE", null);
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ResolveSqlConnectionString_AppendsSaPasswordFromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:connection"] =
                    "Server=sqlserver,1433;Database=MedicalCenter;User Id=sa;TrustServerCertificate=True;Encrypt=False",
                ["ConnectionStrings:SaPassword"] = "SecretPassword123!",
            })
            .Build();

        var connectionString = ServiceCollectionExtensions.ResolveSqlConnectionString(configuration);

        connectionString.Should().Contain("Password=SecretPassword123!");
    }

    [Fact]
    public void ResolveSqlConnectionString_UsesExistingPassword_WhenAlreadyPresent()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:connection"] =
                    "Server=sqlserver,1433;Database=MedicalCenter;User Id=sa;Password=InlinePassword123!;TrustServerCertificate=True;Encrypt=False",
            })
            .Build();

        var connectionString = ServiceCollectionExtensions.ResolveSqlConnectionString(configuration);

        connectionString.Should().Contain("Password=InlinePassword123!");
        connectionString.Should().NotContain("Password=Password=");
    }

    [Fact]
    public void FixtureSecretFiles_ExistForUnitTests()
    {
        Directory.Exists(FixturesDirectory).Should().BeTrue();
        File.Exists(Path.Combine(FixturesDirectory, "jwt_secret")).Should().BeTrue();
    }

    private static string CreateTempSecretsDirectory(IReadOnlyDictionary<string, string> secrets)
    {
        var directory = Path.Combine(Path.GetTempPath(), $"docker-secrets-{Guid.NewGuid()}");
        Directory.CreateDirectory(directory);

        foreach (var secret in secrets)
        {
            File.WriteAllText(Path.Combine(directory, secret.Key), secret.Value);
        }

        return directory;
    }
}
