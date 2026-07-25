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
    public void DockerComposeFile_UsesEnvironmentVariablesInsteadOfHardcodedSecrets()
    {
        var compose = File.ReadAllText(Path.Combine(RepoRoot, "docker-compose.yml"));

        compose.Should().Contain("${MSSQL_SA_PASSWORD}");
        compose.Should().Contain("${JWT_SECRET}");
        compose.Should().Contain("${API_PUBLIC_URL");
        compose.Should().NotContain("Password=SuperSecret");
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
    public void EnvExample_DocumentsRequiredVariables()
    {
        var envExample = File.ReadAllText(Path.Combine(RepoRoot, ".env.example"));

        envExample.Should().Contain("MSSQL_SA_PASSWORD=");
        envExample.Should().Contain("JWT_SECRET=");
        envExample.Should().Contain("API_PUBLIC_URL=");
        envExample.Should().Contain("YARP_HOST_PORT=");
        envExample.Should().Contain("FRONTEND_HOST_PORT=");
    }
}
