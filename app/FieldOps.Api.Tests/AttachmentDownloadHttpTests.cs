using System.Net;
using FieldOps.Api.Data;
using FieldOps.Api.Models;
using FieldOps.Api.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FieldOps.Api.Tests;

public class AttachmentDownloadHttpTests
    : IClassFixture<FieldOpsApiFactory>
{
    private readonly FieldOpsApiFactory _factory;
    private readonly HttpClient _client;

    public AttachmentDownloadHttpTests(
        FieldOpsApiFactory factory)
    {
        _factory = factory;
        _factory.EnsureDatabaseCreated();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Customer_CanDownloadOwnAttachment_ReturnsFileContent()
    {
        var identityObjectId = Guid.NewGuid().ToString("N");
        var fileBytes = new byte[] { 1, 2, 3, 4, 5 };

        var (serviceRequestId, attachmentId) =
            await CreateAttachmentAsync(
                identityObjectId,
                fileBytes);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/service-requests/{serviceRequestId}/attachments/{attachmentId}/content");

        request.Headers.Add("X-Test-Role", "Customer");
        request.Headers.Add("X-Test-Object-Id", identityObjectId);

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(
            "application/pdf",
            response.Content.Headers.ContentType?.MediaType);

        var downloadedBytes =
            await response.Content.ReadAsByteArrayAsync();

        Assert.Equal(fileBytes, downloadedBytes);
    }

    [Fact]
    public async Task UnassignedTechnician_CannotDownloadAttachment_ReturnsForbidden()
    {
        var ownerObjectId = Guid.NewGuid().ToString("N");
        var fileBytes = new byte[] { 1, 2, 3, 4, 5 };

        var (serviceRequestId, attachmentId) =
            await CreateAttachmentAsync(
                ownerObjectId,
                fileBytes);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/service-requests/{serviceRequestId}/attachments/{attachmentId}/content");

        request.Headers.Add("X-Test-Role", "Technician");
        request.Headers.Add(
            "X-Test-Object-Id",
            Guid.NewGuid().ToString("N"));

        using var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    private async Task<(int ServiceRequestId, int AttachmentId)>
        CreateAttachmentAsync(
            string ownerObjectId,
            byte[] fileBytes)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<FieldOpsDbContext>();

        var customer = new Customer
        {
            Name = "Attachment Download Customer",
            Email = $"{ownerObjectId}@example.com",
            Phone = "1234567890",
            IdentityObjectId = ownerObjectId
        };

        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var serviceRequest = new ServiceRequest
        {
            CustomerId = customer.Id,
            Description = "Attachment download test",
            Status = "New",
            CreatedAtUtc = DateTime.UtcNow
        };

        db.ServiceRequests.Add(serviceRequest);
        await db.SaveChangesAsync();

        var blobPath =
            $"service-requests/{serviceRequest.Id}/{Guid.NewGuid():N}.pdf";

        var storage = scope.ServiceProvider
            .GetRequiredService<InMemoryAttachmentStorage>();

        await using (var stream = new MemoryStream(fileBytes))
        {
            await storage.UploadAsync(
                blobPath,
                stream,
                "application/pdf");
        }

        var attachment = new Attachment
        {
            ServiceRequestId = serviceRequest.Id,
            FileName = "evidence.pdf",
            ContentType = "application/pdf",
            BlobPath = blobPath,
            UploadedBy = ownerObjectId,
            UploadedAtUtc = DateTime.UtcNow,
            FileSize = fileBytes.Length
        };

        db.Attachments.Add(attachment);
        await db.SaveChangesAsync();

        return (serviceRequest.Id, attachment.Id);
    }
}