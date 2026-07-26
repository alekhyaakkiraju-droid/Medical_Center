using System.Net;
using System.Net.Http.Json;
using AngularApi.Controllers;
using AngularApi.DTO;
using AngularApi.Services.Interfaces;
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
    private readonly ContactController _controller;

    public ContactControllerTests()
    {
        _controller = new ContactController(_contactServiceMock.Object)
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
        _contactServiceMock
            .Setup(service => service.SubmitInquiryAsync(It.IsAny<ContactInquiryDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.SubmitInquiry(ContactInquiryFixtures.Valid, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { message = "Your inquiry has been submitted successfully." });
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
        var client = _factory.CreateClient();

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
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/Contact", ContactInquiryFixtures.InvalidEmail);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
