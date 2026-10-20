using System.Net;

namespace FieldOps.Api.Tests;

public class AuthorizationHttpTests
    : IClassFixture<FieldOpsApiFactory>
{
    private readonly HttpClient _client;

    public AuthorizationHttpTests(
        FieldOpsApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ServiceRequests_WithoutCredentials_Returns401()
    {
        using var response = await _client.GetAsync(
            "/api/service-requests");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Assignments_CustomerRole_Returns403()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/assignments");

        request.Headers.Add(
            "X-Test-Role",
            "Customer");

        using var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }
}