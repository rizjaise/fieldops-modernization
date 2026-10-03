namespace FieldOps.Api.Models;

public class AssignmentResponse
{
    public int Id { get; set; }

    public int ServiceRequestId { get; set; }

    public int TechnicianId { get; set; }

    public string TechnicianName { get; set; } = string.Empty;

    public DateTime AssignedAtUtc { get; set; }

    public bool IsActive { get; set; }
}