using System.Net;
using System.Text;
using AngularApi.Options;
using AngularApi.Services.impelementation;
using AngularApi.Tests.Fixtures.Recaptcha;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace AngularApi.Tests.Services;

public class RecaptchaServiceTests
{
    private const string TestSecretKey = "test-secret-key";
    private const string ValidToken = "valid-recaptcha-token";

    [Fact]
    public async Task ValidateTokenAsync_SuccessfulValidationWithHighScore_ReturnsTrue()
    {
        var service = CreateService(RecaptchaApiResponseFixtures.SuccessfulHighScore, HttpStatusCode.OK);

        var result = await service.ValidateTokenAsync(ValidToken);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateTokenAsync_ScoreBelowThreshold_ReturnsFalse()
    {
        var service = CreateService(RecaptchaApiResponseFixtures.SuccessfulLowScore, HttpStatusCode.OK);

        var result = await service.ValidateTokenAsync(ValidToken);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateTokenAsync_InvalidToken_ReturnsFalse()
    {
        var service = CreateService(RecaptchaApiResponseFixtures.InvalidToken, HttpStatusCode.OK);

        var result = await service.ValidateTokenAsync("invalid-token");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateTokenAsync_GoogleApiErrorResponse_ReturnsFalse()
    {
        var service = CreateService(RecaptchaApiResponseFixtures.ApiError, HttpStatusCode.OK);

        var result = await service.ValidateTokenAsync(ValidToken);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateTokenAsync_NetworkFailure_ReturnsFalse()
    {
        var handler = new MockHttpMessageHandler((_, _) =>
            throw new HttpRequestException("Network failure"));
        var service = CreateService(handler);

        var result = await service.ValidateTokenAsync(ValidToken);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateTokenAsync_EmptyToken_ReturnsFalseWithoutCallingGoogle()
    {
        var handler = new MockHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var service = CreateService(handler);

        var result = await service.ValidateTokenAsync(string.Empty);

        result.Should().BeFalse();
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateTokenAsync_SendsSecretAndTokenToGoogle()
    {
        var handler = new MockHttpMessageHandler((request, _) =>
        {
            var body = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            body.Should().Contain($"secret={TestSecretKey}");
            body.Should().Contain($"response={ValidToken}");

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    RecaptchaApiResponseFixtures.SuccessfulHighScore,
                    Encoding.UTF8,
                    "application/json"),
            });
        });
        var service = CreateService(handler);

        await service.ValidateTokenAsync(ValidToken);

        handler.Requests.Should().ContainSingle();
        handler.Requests[0].Method.Should().Be(HttpMethod.Post);
        handler.Requests[0].RequestUri!.ToString().Should()
            .Be("https://www.google.com/recaptcha/api/siteverify");
    }

    private static RecaptchaService CreateService(string responseBody, HttpStatusCode statusCode)
    {
        var handler = new MockHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json"),
            }));

        return CreateService(handler);
    }

    private static RecaptchaService CreateService(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var httpClientFactory = new StubHttpClientFactory(httpClient);
        var settings = Microsoft.Extensions.Options.Options.Create(new RecaptchaSettings
        {
            SecretKey = TestSecretKey,
            MinimumScore = 0.5,
            Enabled = true,
        });

        return new RecaptchaService(
            httpClientFactory,
            settings,
            NullLogger<RecaptchaService>.Instance);
    }

    private sealed class StubHttpClientFactory(HttpClient httpClient) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => httpClient;
    }
}
