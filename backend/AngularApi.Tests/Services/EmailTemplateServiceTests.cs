using AngularApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace AngularApi.Tests.Services;

public class EmailTemplateServiceTests
{
    [Fact]
    public void GetAppointmentConfirmationEmail_CachesTemplateAfterFirstLoad()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var templateDir = Path.Combine(tempDir, "EmailTemplates");
        Directory.CreateDirectory(templateDir);
        var templatePath = Path.Combine(templateDir, "ConfirmAppointment.html");
        File.WriteAllText(templatePath, "Hello {{patientName}} with {{DoctorName}} on {{date}}");

        var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
        webHostEnvironmentMock.Setup(env => env.WebRootPath).Returns(tempDir);

        var service = new EmailTemplateService(webHostEnvironmentMock.Object);

        var first = service.GetAppointmentConfirmationEmail("Jane", "Dr Smith", "2026-07-23");
        File.WriteAllText(templatePath, "Changed template content");
        var second = service.GetAppointmentConfirmationEmail("Jane", "Dr Smith", "2026-07-23");

        first.Should().Be(second);
        first.Should().Contain("Jane");
        first.Should().Contain("Dr Smith");

        Directory.Delete(tempDir, true);
    }
}
