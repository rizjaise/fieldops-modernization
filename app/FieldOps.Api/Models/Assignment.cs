namespace FieldOps.Api.Models;

public class Assignment
{
    public int Id { get; set; }

    public int ServiceRequestId { get; set; }

    public ServiceRequest ServiceRequest { get; set; } = null!;

    public int TechnicianId { get; set; }

    public Technician Technician { get; set; } = null!;

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UnassignedAtUtc { get; set; }

    public bool IsActive { get; set; } = true;
}