namespace FieldOps.Api.Models;

public class Attachment
{
    public int Id { get; set; }

    public int ServiceRequestId { get; set; }

    public ServiceRequest ServiceRequest { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string BlobPath { get; set; } = string.Empty;

    public string UploadedBy { get; set; } = string.Empty;

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    public long FileSize { get; set; }
}