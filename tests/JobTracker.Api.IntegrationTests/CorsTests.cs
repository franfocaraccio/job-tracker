using Microsoft.AspNetCore.Mvc.Testing;

namespace JobTracker.Api.IntegrationTests;

public sealed class CorsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CorsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Preflight_FromTheFrontendOrigin_IsAllowed()
    {
        var response = await _client.SendAsync(PreflightRequest("http://localhost:4200"));

        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins));
        Assert.Equal("http://localhost:4200", origins.Single());
    }

    [Fact]
    public async Task Preflight_FromAnUnknownOrigin_IsNotAllowed()
    {
        var response = await _client.SendAsync(PreflightRequest("http://unknown-site.com"));

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private static HttpRequestMessage PreflightRequest(string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/auth/login");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        return request;
    }
}
