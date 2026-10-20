using System.Net;
using FieldOps.Api.Data;
using FieldOps.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace FieldOps.Api.Tests;

public class ResourceAuthorizationHttpTests
    : IClassFixture<FieldOpsApiFactory>
{
    private readonly FieldOpsApiFactory _factory;
    private readonly HttpClient _client;

    public ResourceAuthorizationHttpTests(
        FieldOpsApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        _factory.EnsureDatabaseCreated();
    }

    [Fact]
    public async Task Customer_CanGetOwnServiceRequest_ReturnsOk()
    {
        var ownerObjectId = Guid.NewGuid().ToString("N");

        var serviceRequestId =
            await CreateServiceRequestAsync(ownerObjectId);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/service-requests/{serviceRequestId}");

        request.Headers.Add("X-Test-Role", "Customer");
        request.Headers.Add("X-Test-Object-Id", ownerObjectId);

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Customer_CannotGetAnotherCustomersRequest_ReturnsForbidden()
    {
        var ownerObjectId = Guid.NewGuid().ToString("N");
        var otherCustomerObjectId = Guid.NewGuid().ToString("N");

        var serviceRequestId =
            await CreateServiceRequestAsync(ownerObjectId);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/service-requests/{serviceRequestId}");

        request.Headers.Add("X-Test-Role", "Customer");
        request.Headers.Add(
            "X-Test-Object-Id",
            otherCustomerObjectId);

        using var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task Customer_GetAll_ReturnsOnlyOwnServiceRequests()
    {
        var ownObjectId = Guid.NewGuid().ToString("N");
        var otherObjectId = Guid.NewGuid().ToString("N");

        var ownRequestId =
            await CreateServiceRequestAsync(ownObjectId);

        await CreateServiceRequestAsync(otherObjectId);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/service-requests");

        request.Headers.Add("X-Test-Role", "Customer");
        request.Headers.Add("X-Test-Object-Id", ownObjectId);

        using var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var requests =
            await response.Content
                .ReadFromJsonAsync<List<ServiceRequestResponse>>();

        Assert.NotNull(requests);
        Assert.Single(requests);
        Assert.Equal(ownRequestId, requests[0].Id);
    }

    [Fact]
    public async Task Customer_CanCreateOwnServiceRequest_ReturnsCreated()
    {
        var identityObjectId = Guid.NewGuid().ToString("N");

        var customerId =
            await CreateCustomerAsync(identityObjectId);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/service-requests");

        request.Headers.Add("X-Test-Role", "Customer");
        request.Headers.Add("X-Test-Object-Id", identityObjectId);

        request.Content = System.Net.Http.Json.JsonContent.Create(
            new
            {
                customerId,
                description = "Integration test service request"
            });

        using var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }

    [Fact]
    public async Task Customer_CannotCreateRequestForAnotherCustomer_ReturnsForbidden()
    {
        var authenticatedIdentity = Guid.NewGuid().ToString("N");
        var otherIdentity = Guid.NewGuid().ToString("N");

        var otherCustomerId =
            await CreateCustomerAsync(otherIdentity);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/service-requests");

        request.Headers.Add("X-Test-Role", "Customer");
        request.Headers.Add("X-Test-Object-Id", authenticatedIdentity);

        request.Content = System.Net.Http.Json.JsonContent.Create(
            new
            {
                customerId = otherCustomerId,
                description = "Unauthorized integration test request"
            });

        using var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task UnassignedTechnician_CannotUpdateServiceRequestStatus_ReturnsForbidden()
    {
        var customerObjectId = Guid.NewGuid().ToString("N");
        var serviceRequestId =
            await CreateServiceRequestAsync(customerObjectId);

        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"/api/service-requests/{serviceRequestId}/status");

        request.Headers.Add("X-Test-Role", "Technician");
        request.Headers.Add(
            "X-Test-Object-Id",
            Guid.NewGuid().ToString("N"));

        request.Content = System.Net.Http.Json.JsonContent.Create(
            new
            {
                status = "Assigned"
            });

        using var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    private async Task<int> CreateServiceRequestAsync(
        string ownerObjectId)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<FieldOpsDbContext>();

        var customer = new Customer
        {
            Name = "Integration Test Customer",
            Email = $"{ownerObjectId}@example.com",
            Phone = "1234567890",
            IdentityObjectId = ownerObjectId
        };

        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var serviceRequest = new ServiceRequest
        {
            CustomerId = customer.Id,
            Description = "Integration test request",
            Status = "New",
            CreatedAtUtc = DateTime.UtcNow
        };

        db.ServiceRequests.Add(serviceRequest);
        await db.SaveChangesAsync();

        return serviceRequest.Id;
    }

    private async Task<int> CreateCustomerAsync(
    string identityObjectId)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<FieldOpsDbContext>();

        var customer = new Customer
        {
            Name = "Integration Test Customer",
            Email = $"{identityObjectId}@example.com",
            Phone = "1234567890",
            IdentityObjectId = identityObjectId
        };

        db.Customers.Add(customer);

        await db.SaveChangesAsync();

        return customer.Id;
    }
}