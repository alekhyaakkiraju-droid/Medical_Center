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

    [Fact]
    public async Task SwaggerEndpoint_ContainsExpectedDtoSchemas()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        response.IsSuccessStatusCode.Should().BeTrue();

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");

        foreach (var schema in new[]
        {
            "DoctorDTO",
            "AppointmentDTO",
            "PatientDTO",
            "ReviewDTO",
            "UpdateAppointmentDTO",
            "RegisterUserDTO",
            "LogInUserDTO",
            "UpdateProfileDto",
        })
        {
            schemas.TryGetProperty(schema, out _).Should().BeTrue(because: $"OpenAPI spec should expose {schema}");
        }
    }

    [Fact]
    public async Task ExportOpenApiSpec_WritesValidJsonWithSchemas()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"swagger-export-{Guid.NewGuid():N}.json");
        try
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/swagger/v1/swagger.json");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            await File.WriteAllTextAsync(outputPath, json);

            File.Exists(outputPath).Should().BeTrue();
            new FileInfo(outputPath).Length.Should().BeGreaterThan(0);

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            document.RootElement.GetProperty("openapi").GetString().Should().StartWith("3.");
            document.RootElement.GetProperty("components").GetProperty("schemas").EnumerateObject().Should().NotBeEmpty();
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }
}
