using FieldOps.Api.Data;
using FieldOps.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FieldOps.Api.Services;

namespace FieldOps.Api.Controllers;

[ApiController]
[Route("api/service-requests/{serviceRequestId:int}/attachments")]
public class AttachmentsController : ControllerBase
{
    private readonly FieldOpsDbContext _db;

    public AttachmentsController(FieldOpsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AttachmentResponse>>> GetAll(
        int serviceRequestId)
    {
        var serviceRequestExists = await _db.ServiceRequests
            .AnyAsync(x => x.Id == serviceRequestId);

        if (!serviceRequestExists)
        {
            return NotFound(
                $"Service request with ID {serviceRequestId} does not exist.");
        }

        var attachments = await _db.Attachments
            .AsNoTracking()
            .Where(x => x.ServiceRequestId == serviceRequestId)
            .OrderByDescending(x => x.UploadedAtUtc)
            .Select(x => new AttachmentResponse
            {
                Id = x.Id,
                ServiceRequestId = x.ServiceRequestId,
                FileName = x.FileName,
                ContentType = x.ContentType,
                FileSize = x.FileSize,
                UploadedBy = x.UploadedBy,
                UploadedAtUtc = x.UploadedAtUtc
            })
            .ToListAsync();

        return Ok(attachments);
    }

    [HttpPost]
[RequestSizeLimit(10 * 1024 * 1024)]
public async Task<ActionResult<AttachmentResponse>> Upload(
    int serviceRequestId,
    IFormFile file,
    [FromServices] IAttachmentStorage storage,
    CancellationToken cancellationToken)
{
    if (file is null || file.Length == 0)
    {
        return BadRequest("A non-empty file is required.");
    }

    var serviceRequestExists = await _db.ServiceRequests
        .AnyAsync(
            x => x.Id == serviceRequestId,
            cancellationToken);

    if (!serviceRequestExists)
    {
        return NotFound(
            $"Service request with ID {serviceRequestId} does not exist.");
    }

    const long maxFileSize = 10 * 1024 * 1024;

    if (file.Length > maxFileSize)
    {
        return BadRequest(
            "Attachment exceeds the maximum allowed size of 10 MB.");
    }

    var allowedContentTypes = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "application/pdf"
    };

    if (!allowedContentTypes.Contains(file.ContentType))
    {
        return BadRequest(
            "Only JPEG, PNG, and PDF attachments are supported.");
    }

    var extension = Path.GetExtension(file.FileName);

    if (string.IsNullOrWhiteSpace(extension))
    {
        return BadRequest("The uploaded file must have an extension.");
    }

    var attachmentId = Guid.NewGuid().ToString("N");

    var blobPath =
        $"service-requests/{serviceRequestId}/{attachmentId}{extension}";

    await using var stream = file.OpenReadStream();

    await storage.UploadAsync(
        blobPath,
        stream,
        file.ContentType,
        cancellationToken);

    var attachment = new Attachment
    {
        ServiceRequestId = serviceRequestId,
        FileName = Path.GetFileName(file.FileName),
        ContentType = file.ContentType,
        BlobPath = blobPath,
        UploadedBy = "system",
        UploadedAtUtc = DateTime.UtcNow,
        FileSize = file.Length
    };

    _db.Attachments.Add(attachment);

    await _db.SaveChangesAsync(cancellationToken);

    var response = new AttachmentResponse
    {
        Id = attachment.Id,
        ServiceRequestId = attachment.ServiceRequestId,
        FileName = attachment.FileName,
        ContentType = attachment.ContentType,
        FileSize = attachment.FileSize,
        UploadedBy = attachment.UploadedBy,
        UploadedAtUtc = attachment.UploadedAtUtc
    };

    return Ok(response);
}   
}