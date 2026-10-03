namespace FieldOps.Api.Models;

public class AttachmentResponse
{
    public int Id { get; set; }

    public int ServiceRequestId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string UploadedBy { get; set; } = string.Empty;

    public DateTime UploadedAtUtc { get; set; }
}