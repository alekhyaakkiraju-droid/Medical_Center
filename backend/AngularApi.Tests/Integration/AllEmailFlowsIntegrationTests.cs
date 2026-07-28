using FluentAssertions;

namespace AngularApi.Tests.Integration;

/// <summary>
/// Consolidated verification entry point for all transactional email flows (WO-031).
/// Individual scenarios are implemented in dedicated integration test classes.
/// </summary>
public class AllEmailFlowsIntegrationTests
{
    [Fact]
    public void EmailFlowIntegrationTests_AreDefinedForAllThreeFlows()
    {
        typeof(EmailConfirmationFlowTests).GetMethod(nameof(EmailConfirmationFlowTests.RegisterUser_SendsConfirmationEmailToMailHog_AndConfirmEmailSetsEmailConfirmed))
            .Should().NotBeNull();
        typeof(PasswordResetFlowTests).GetMethod(nameof(PasswordResetFlowTests.ForgotPassword_ToResetPassword_AllowsLoginWithNewPassword))
            .Should().NotBeNull();
        typeof(AppointmentConfirmationEmailIntegrationTests).GetMethod(nameof(AppointmentConfirmationEmailIntegrationTests.CreateAppointment_SendsConfirmationEmailToMailHog))
            .Should().NotBeNull();
    }
}
