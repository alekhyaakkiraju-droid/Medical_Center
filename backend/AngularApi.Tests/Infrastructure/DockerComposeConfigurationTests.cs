using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class DockerComposeConfigurationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact]
    public void DockerComposeFile_DefinesRequiredServices()
    {
        var compose = File.ReadAllText(Path.Combine(RepoRoot, "docker-compose.yml"));

        compose.Should().Contain("angular-frontend:");
        compose.Should().Contain("api:");
        compose.Should().Contain("yarp-proxy:");
        compose.Should().Contain("sqlserver:");
    }

    [Fact]
    public void DockerComposeFile_UsesDockerSecretsInsteadOfPlainSecretEnvironmentVariables()
    {
        var compose = File.ReadAllText(Path.Combine(RepoRoot, "docker-compose.yml"));

        compose.Should().Contain("secrets:");
        compose.Should().Contain("jwt_secret:");
        compose.Should().Contain("mssql_sa_password:");
        compose.Should().Contain("smtp_email_username:");
        compose.Should().Contain("smtp_email_password:");
        compose.Should().Contain("google_auth_client_id:");
        compose.Should().Contain("google_auth_client_secret:");
        compose.Should().NotContain("Jwt__Secret: ${JWT_SECRET}");
        compose.Should().NotContain("EmailSettings__EmailUsername: ${SMTP_EMAIL_USERNAME}");
        compose.Should().NotContain("EmailSettings__EmailPassword: ${SMTP_EMAIL_PASSWORD}");
        compose.Should().NotContain("MSSQL_SA_PASSWORD: ${MSSQL_SA_PASSWORD}");
        compose.Should().NotContain("GoogleAuth__ClientId: ${GOOGLE_AUTH_CLIENT_ID}");
        compose.Should().NotContain("GoogleAuth__ClientSecret: ${GOOGLE_AUTH_CLIENT_SECRET}");
    }

    [Fact]
    public void DockerComposeFile_MountsSecretsOnApiService()
    {
        var compose = File.ReadAllText(Path.Combine(RepoRoot, "docker-compose.yml"));

        compose.Should().Contain("secrets:");
        compose.Should().Contain("- jwt_secret");
        compose.Should().Contain("- mssql_sa_password");
        compose.Should().Contain("- smtp_email_username");
        compose.Should().Contain("- smtp_email_password");
        compose.Should().Contain("- google_auth_client_id");
        compose.Should().Contain("- google_auth_client_secret");
    }

    [Fact]
    public void DockerComposeFile_WiresHealthCheckDependencies()
    {
        var compose = File.ReadAllText(Path.Combine(RepoRoot, "docker-compose.yml"));

        compose.Should().Contain("depends_on:");
        compose.Should().Contain("condition: service_healthy");
        compose.Should().Contain("healthcheck:");
    }

    [Fact]
    public void DockerComposeFile_UsesSqlServer2022Image()
    {
        var compose = File.ReadAllText(Path.Combine(RepoRoot, "docker-compose.yml"));

        compose.Should().Contain("mcr.microsoft.com/mssql/server:2022-latest");
    }

    [Fact]
    public void YarpConfig_RoutesApiTrafficToApiService()
    {
        var yarpConfig = File.ReadAllText(Path.Combine(RepoRoot, "docker/yarp.config.json"));

        yarpConfig.Should().Contain("/api/{**catch-all}");
        yarpConfig.Should().Contain("http://api:8080");
    }

    [Fact]
    public void DockerComposeFile_ConfiguresCorsAllowedOrigins()
    {
        var compose = File.ReadAllText(Path.Combine(RepoRoot, "docker-compose.yml"));

        compose.Should().Contain("CorsSettings__AllowedOrigins__0:");
        compose.Should().Contain("CorsSettings__AllowedOrigins__1:");
    }

    [Fact]
    public void EnvExample_DocumentsCorsAllowedOrigins()
    {
        var envExample = File.ReadAllText(Path.Combine(RepoRoot, ".env.example"));

        envExample.Should().Contain("CORS_ALLOWED_ORIGINS_0=");
        envExample.Should().Contain("CORS_ALLOWED_ORIGINS_1=");
        envExample.Should().Contain("CorsSettings__AllowedOrigins__N");
    }

    [Fact]
    public void EnvExample_DocumentsDockerSecretsWorkflow()
    {
        var envExample = File.ReadAllText(Path.Combine(RepoRoot, ".env.example"));

        envExample.Should().Contain("secrets.example");
        envExample.Should().NotContain("JWT_SECRET=ChangeMe");
        envExample.Should().NotContain("MSSQL_SA_PASSWORD=ChangeMe");
        envExample.Should().Contain("API_PUBLIC_URL=");
        envExample.Should().Contain("YARP_HOST_PORT=");
        envExample.Should().Contain("FRONTEND_HOST_PORT=");
    }
}
