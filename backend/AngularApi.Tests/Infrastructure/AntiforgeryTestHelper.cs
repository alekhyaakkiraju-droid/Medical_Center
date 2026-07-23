using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AngularApi.Tests.Infrastructure;

public static class AntiforgeryTestHelper
{
    public static async Task ApplyAntiforgeryTokenAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/Account/antiforgery-token");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<AntiforgeryTokenResponse>();
        payload.Should().NotBeNull();
        payload!.Token.Should().NotBeNullOrWhiteSpace();

        client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", payload.Token);
    }

    public static HttpClient CreateClient(MedicalCenterWebApplicationFactory factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });
    }

    private record AntiforgeryTokenResponse(string Token);

    public static void ImportAuthCookies(HttpResponseMessage response, HttpClient client)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookies))
        {
            return;
        }

        client.DefaultRequestHeaders.Remove("Cookie");
        var cookiePairs = setCookies.Select(value => value.Split(';')[0]);
        client.DefaultRequestHeaders.Add("Cookie", string.Join("; ", cookiePairs));
    }
}
