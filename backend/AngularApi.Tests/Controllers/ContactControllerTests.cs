using System.Net;
using System.Net.Http.Json;
using AngularApi.Controllers;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Services.Interfaces;
using AngularApi.Tests.Fixtures.Recaptcha;
using AngularApi.Tests.Infrastructure;
using AngularApi.Tests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AngularApi.Tests.Controllers;

public class ContactControllerTests
{
    private readonly Mock<IContactService> _contactServiceMock = new();
    private readonly Mock<IRecaptchaService> _recaptchaServiceMock = new();
    private readonly ContactController _controller;

    public ContactControllerTests()
    {
        _recaptchaServiceMock
            .Setup(service => service.ValidateTokenAsync(RecaptchaTokenFixtures.Valid))
            .ReturnsAsync(true);

        _controller = new ContactController(_contactServiceMock.Object, _recaptchaServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            },
        };
    }

    [Fact]
    public async Task SubmitInquiry_ValidInput_ReturnsOkWithSuccessMessage()
    {
        var dto = ContactInquiryFixtures.Valid;
        _contactServiceMock
            .Setup(service => service.SubmitInquiryAsync(It.IsAny<ContactInquiryDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.SubmitInquiry(dto, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { message = "Your inquiry has been submitted successfully." });
        _recaptchaServiceMock.Verify(
            service => service.ValidateTokenAsync(dto.RecaptchaToken),
            Times.Once);
        _contactServiceMock.Verify(
            service => service.SubmitInquiryAsync(
                It.Is<ContactInquiryDTO>(inquiry => inquiry.RecaptchaToken == dto.RecaptchaToken),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SubmitInquiry_InvalidRecaptchaToken_ReturnsBadRequest()
    {
        _recaptchaServiceMock
            .Setup(service => service.ValidateTokenAsync(RecaptchaTokenFixtures.Invalid))
            .ReturnsAsync(false);

        var result = await _controller.SubmitInquiry(ContactInquiryFixtures.InvalidRecaptchaToken, CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeEquivalentTo(new { error = "reCAPTCHA validation failed" });
        _contactServiceMock.Verify(
            service => service.SubmitInquiryAsync(It.IsAny<ContactInquiryDTO>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SubmitInquiry_ServiceFailure_ReturnsInternalServerError()
    {
        _contactServiceMock
            .Setup(service => service.SubmitInquiryAsync(It.IsAny<ContactInquiryDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.SubmitInquiry(ContactInquiryFixtures.Valid, CancellationToken.None);

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}

public class ContactControllerRateLimitTests : IClassFixture<MedicalCenterWebApplicationFactory>
{
    private readonly MedicalCenterWebApplicationFactory _factory;

    public ContactControllerRateLimitTests(MedicalCenterWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SubmitInquiry_ExceedingThreeRequestsPerMinute_ReturnsTooManyRequests()
    {
        var client = AntiforgeryTestHelper.CreateClient(_factory);
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        for (var attempt = 0; attempt < 3; attempt++)
        {
            var response = await client.PostAsJsonAsync("/api/Contact", ContactInquiryFixtures.Valid);
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }

        var limitedResponse = await client.PostAsJsonAsync("/api/Contact", ContactInquiryFixtures.Valid);
        limitedResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}

public class ContactControllerValidationTests
{
    [Fact]
    public async Task SubmitInquiry_InvalidPayload_ReturnsBadRequest()
    {
        await using var factory = new MedicalCenterWebApplicationFactory();
        var client = AntiforgeryTestHelper.CreateClient(factory);
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var response = await client.PostAsJsonAsync("/api/Contact", ContactInquiryFixtures.InvalidEmail);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SubmitInquiry_MissingRecaptchaToken_ReturnsBadRequest()
    {
        await using var factory = new MedicalCenterWebApplicationFactory();
        var client = AntiforgeryTestHelper.CreateClient(factory);
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);

        var response = await client.PostAsJsonAsync("/api/Contact", ContactInquiryFixtures.MissingRecaptchaToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
