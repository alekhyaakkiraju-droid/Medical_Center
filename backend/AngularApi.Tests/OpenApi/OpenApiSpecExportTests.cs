using System.Text.Json;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;

namespace AngularApi.Tests.OpenApi;

public class OpenApiSpecExportTests : IClassFixture<MedicalCenterWebApplicationFactory>
{
    private readonly MedicalCenterWebApplicationFactory _factory;

    public OpenApiSpecExportTests(MedicalCenterWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SwaggerEndpoint_ReturnsOpenApi30Document()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        response.IsSuccessStatusCode.Should().BeTrue();
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("openapi").GetString().Should().StartWith("3.");
        document.RootElement.GetProperty("paths").EnumerateObject().Should().NotBeEmpty();
    }

    [Fact]
    public async Task ExportOpenApiSpec_WhenOutputPathConfigured()
    {
        var outputPath = Environment.GetEnvironmentVariable("OPENAPI_OUTPUT_PATH");
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            return;
        }

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        await File.WriteAllTextAsync(outputPath, json);

        File.Exists(outputPath).Should().BeTrue();
    }
}
