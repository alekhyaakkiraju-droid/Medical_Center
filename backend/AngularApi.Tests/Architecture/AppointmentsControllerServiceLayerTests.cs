using AngularApi.Controllers;
using AngularApi.Contracts.Services.Interfaces;
using AngularApi.Services;
using FluentAssertions;
using System.Reflection;

namespace AngularApi.Tests.Architecture;

public class AppointmentsControllerServiceLayerTests
{
    [Fact]
    public void AppointmentsController_DoesNotDependOnEmailServices()
    {
        var emailServiceFields = typeof(AppointmentsController)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(field => field.FieldType == typeof(IEmailService) || field.FieldType == typeof(EmailTemplateService))
            .ToList();

        emailServiceFields.Should().BeEmpty(because: "appointment confirmation email belongs in AppointmentService");
    }

    [Fact]
    public void AppointmentsController_Constructor_OnlyAcceptsAppointmentServiceAndUserManager()
    {
        var parameters = typeof(AppointmentsController)
            .GetConstructors()
            .Single()
            .GetParameters()
            .Select(parameter => parameter.ParameterType)
            .ToArray();

        parameters.Should().BeEquivalentTo([
            typeof(IAppointmentService),
            typeof(Microsoft.AspNetCore.Identity.UserManager<AngularApi.Contracts.Models.AppUser>),
        ]);
    }
}
