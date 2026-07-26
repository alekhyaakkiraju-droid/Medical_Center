using System.Text.Json;
using System.Xml.Linq;
using FluentAssertions;

namespace AngularApi.Tests.Infrastructure;

public class DotNet10UpgradeConfigurationTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string[] BackendProjects =
    [
        "backend/AngularApi/AngularApi.csproj",
        "backend/YARPReverseProxy/YARPReverseProxy.csproj",
        "backend/AngularApi.Tests/AngularApi.Tests.csproj",
    ];

    [Fact]
    public void GlobalJson_PinsDotNet10Sdk()
    {
        var globalJsonPath = Path.Combine(RepoRoot, "global.json");
        File.Exists(globalJsonPath).Should().BeTrue(because: "WO-045 requires a root global.json pinning the .NET 10 SDK");

        using var document = JsonDocument.Parse(File.ReadAllText(globalJsonPath));
        var sdk = document.RootElement.GetProperty("sdk");

        sdk.GetProperty("version").GetString().Should().StartWith("10.0.");
        sdk.GetProperty("rollForward").GetString().Should().Be("latestFeature");
        sdk.GetProperty("allowPrerelease").GetBoolean().Should().BeFalse();
    }

    [Theory]
    [InlineData("backend/AngularApi/AngularApi.csproj")]
    [InlineData("backend/YARPReverseProxy/YARPReverseProxy.csproj")]
    [InlineData("backend/AngularApi.Tests/AngularApi.Tests.csproj")]
    public void BackendProjects_TargetNet10(string relativePath)
    {
        var project = XDocument.Load(Path.Combine(RepoRoot, relativePath));
        var targetFramework = project.Root!
            .Element("PropertyGroup")!
            .Element("TargetFramework")!
            .Value;

        targetFramework.Should().Be("net10.0", because: $"{relativePath} must target .NET 10 for WO-045");
    }

    [Fact]
    public void AngularApiProject_ReferencesDotNet10MicrosoftPackages()
    {
        var project = XDocument.Load(Path.Combine(RepoRoot, "backend/AngularApi/AngularApi.csproj"));
        var packageReferences = project.Root!
            .Elements("ItemGroup")
            .SelectMany(group => group.Elements("PackageReference"))
            .ToDictionary(
                element => element.Attribute("Include")!.Value,
                element => element.Attribute("Version")!.Value);

        packageReferences["Microsoft.EntityFrameworkCore.SqlServer"].Should().StartWith("10.");
        packageReferences["Microsoft.AspNetCore.Identity.EntityFrameworkCore"].Should().StartWith("10.");
        packageReferences["Microsoft.AspNetCore.Authentication.JwtBearer"].Should().StartWith("10.");
        packageReferences["Microsoft.AspNetCore.Authentication.Google"].Should().StartWith("10.");
    }

    [Fact]
    public void TestProject_ReferencesDotNet10TestingPackages()
    {
        var project = XDocument.Load(Path.Combine(RepoRoot, "backend/AngularApi.Tests/AngularApi.Tests.csproj"));
        var packageReferences = project.Root!
            .Elements("ItemGroup")
            .SelectMany(group => group.Elements("PackageReference"))
            .ToDictionary(
                element => element.Attribute("Include")!.Value,
                element => element.Attribute("Version")!.Value);

        packageReferences["Microsoft.AspNetCore.Mvc.Testing"].Should().StartWith("10.");
        packageReferences["Microsoft.EntityFrameworkCore.InMemory"].Should().StartWith("10.");
        packageReferences.Should().ContainKey("FluentValidation");
    }

    [Fact]
    public void YarpProject_ReferencesDotNet10CompatibleReverseProxyPackage()
    {
        var project = XDocument.Load(Path.Combine(RepoRoot, "backend/YARPReverseProxy/YARPReverseProxy.csproj"));
        var packageReferences = project.Root!
            .Elements("ItemGroup")
            .SelectMany(group => group.Elements("PackageReference"))
            .ToDictionary(
                element => element.Attribute("Include")!.Value,
                element => element.Attribute("Version")!.Value);

        packageReferences.Should().ContainKey("Yarp.ReverseProxy");
        packageReferences["Yarp.ReverseProxy"].Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void DotNetToolsManifest_ReferencesDotNet10CompatibleSwashbuckleCli()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(RepoRoot, ".config/dotnet-tools.json")));
        var version = document.RootElement
            .GetProperty("tools")
            .GetProperty("swashbuckle.aspnetcore.cli")
            .GetProperty("version")
            .GetString();

        version.Should().StartWith("10.");
    }

    [Fact]
    public void BackendProjects_DoNotReferenceNet8TargetFramework()
    {
        foreach (var relativePath in BackendProjects)
        {
            var contents = File.ReadAllText(Path.Combine(RepoRoot, relativePath));
            contents.Should().NotContain("net8.0", because: $"{relativePath} must not retain .NET 8 targeting after WO-045");
        }
    }
}
