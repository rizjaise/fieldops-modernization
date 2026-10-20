namespace FieldOps.Api.Tests;

public class ApiSmokeTests
    : IClassFixture<FieldOpsApiFactory>
{
    private readonly HttpClient _client;

    public ApiSmokeTests(FieldOpsApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        using var response =
            await _client.GetAsync("/health");

        var body =
            await response.Content.ReadAsStringAsync();

        Assert.Equal(
            System.Net.HttpStatusCode.OK,
            response.StatusCode);

        Assert.Equal("Healthy", body.Trim());
    }

    [Fact]
    public async Task RootEndpoint_ReturnsApiName()
    {
        using var response =
            await _client.GetAsync("/");

        var body =
            await response.Content.ReadAsStringAsync();

        Assert.Equal(
            System.Net.HttpStatusCode.OK,
            response.StatusCode);

        Assert.Equal("FieldOps API", body);
    }
}