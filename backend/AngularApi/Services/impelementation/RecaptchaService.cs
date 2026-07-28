using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using AngularApi.Options;
using AngularApi.Contracts.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace AngularApi.Services.impelementation
{
    public class RecaptchaService : IRecaptchaService
    {
        internal const string HttpClientName = "RecaptchaService";
        private const string SiteVerifyUrl = "https://www.google.com/recaptcha/api/siteverify";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly RecaptchaSettings _settings;
        private readonly ILogger<RecaptchaService> _logger;

        public RecaptchaService(
            IHttpClientFactory httpClientFactory,
            IOptions<RecaptchaSettings> settings,
            ILogger<RecaptchaService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            if (!_settings.Enabled)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            try
            {
                var client = _httpClientFactory.CreateClient(HttpClientName);
                using var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = _settings.SecretKey,
                    ["response"] = token,
                });
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                using var response = await client.PostAsync(SiteVerifyUrl, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var verification = JsonSerializer.Deserialize<RecaptchaVerificationResponse>(responseBody);

                return verification?.Success == true
                    && verification.Score >= _settings.MinimumScore;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "reCAPTCHA validation failed due to an HTTP or parsing error.");
                return false;
            }
        }

        private sealed class RecaptchaVerificationResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("score")]
            public double Score { get; set; }
        }
    }
}
