using System.Net;
using System.Net.Http.Headers;
using FieldOps.Api.Data;
using FieldOps.Api.Models;
using FieldOps.Api.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FieldOps.Api.Tests;

public class AttachmentHttpTests
    : IClassFixture<FieldOpsApiFactory>
{
    private readonly FieldOpsApiFactory _factory;
    private readonly HttpClient _client;

    public AttachmentHttpTests(FieldOpsApiFactory factory)
    {
        _factory = factory;
        _factory.EnsureDatabaseCreated();

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Dispatcher_CanUploadValidAttachment_ReturnsOk()
    {
        var serviceRequestId =
            await CreateServiceRequestAsync();

        var storage = _factory.Services
            .GetRequiredService<InMemoryAttachmentStorage>();

        var previousFileCount = storage.FileCount;

        using var request = CreateUploadRequest(
            serviceRequestId,
            "Dispatcher",
            "image/jpeg",
            "photo.jpg");

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(previousFileCount + 1, storage.FileCount);
    }

    [Fact]
    public async Task UnsupportedContentType_ReturnsBadRequest_WithoutStoringFile()
    {
        var serviceRequestId =
            await CreateServiceRequestAsync();

        var storage = _factory.Services
            .GetRequiredService<InMemoryAttachmentStorage>();

        var previousFileCount = storage.FileCount;

        using var request = CreateUploadRequest(
            serviceRequestId,
            "Dispatcher",
            "text/plain",
            "notes.txt");

        using var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.Equal(previousFileCount, storage.FileCount);
    }

    [Fact]
    public async Task UnassignedTechnician_CannotUploadAttachment_ReturnsForbidden()
    {
        var serviceRequestId =
            await CreateServiceRequestAsync();

        var storage = _factory.Services
            .GetRequiredService<InMemoryAttachmentStorage>();

        var previousFileCount = storage.FileCount;

        using var request = CreateUploadRequest(
            serviceRequestId,
            "Technician",
            "image/jpeg",
            "photo.jpg");

        using var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

        Assert.Equal(previousFileCount, storage.FileCount);
    }

    private static HttpRequestMessage CreateUploadRequest(
        int serviceRequestId,
        string role,
        string contentType,
        string fileName)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/service-requests/{serviceRequestId}/attachments");

        request.Headers.Add("X-Test-Role", role);
        request.Headers.Add(
            "X-Test-Object-Id",
            $"test-{Guid.NewGuid():N}");

        var multipart = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent(
            [0xFF, 0xD8, 0xFF, 0xD9]);

        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(contentType);

        multipart.Add(fileContent, "file", fileName);

        request.Content = multipart;

        return request;
    }

    private async Task<int> CreateServiceRequestAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<FieldOpsDbContext>();

        var customer = new Customer
        {
            Name = "Attachment Test Customer",
            Email = $"{Guid.NewGuid():N}@example.com",
            Phone = "1234567890",
            IdentityObjectId = Guid.NewGuid().ToString("N")
        };

        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var serviceRequest = new ServiceRequest
        {
            CustomerId = customer.Id,
            Description = "Attachment integration test",
            Status = "New",
            CreatedAtUtc = DateTime.UtcNow
        };

        db.ServiceRequests.Add(serviceRequest);
        await db.SaveChangesAsync();

        return serviceRequest.Id;
    }
}