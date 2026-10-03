namespace FieldOps.Api.Models;

public class StatusHistory
{
    public int Id { get; set; }

    public int ServiceRequestId { get; set; }

    public string FromStatus { get; set; } = string.Empty;

    public string ToStatus { get; set; } = string.Empty;

    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;

    public string ChangedBy { get; set; } = string.Empty;
}