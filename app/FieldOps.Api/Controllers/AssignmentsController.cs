using FieldOps.Api.Data;
using FieldOps.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FieldOps.Api.Services;

namespace FieldOps.Api.Controllers;

[ApiController]
[Route("api/assignments")]
public class AssignmentsController : ControllerBase
{
    private readonly FieldOpsDbContext _db;

    public AssignmentsController(FieldOpsDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<AssignmentResponse>> Create(
        CreateAssignmentRequest request)
    {
        var serviceRequest = await _db.ServiceRequests
            .FirstOrDefaultAsync(x => x.Id == request.ServiceRequestId);

        if (serviceRequest is null)
        {
            return NotFound(
                $"Service request with ID {request.ServiceRequestId} does not exist.");
        }

        var technician = await _db.Technicians
            .FirstOrDefaultAsync(x => x.Id == request.TechnicianId);

        if (technician is null)
        {
            return NotFound(
                $"Technician with ID {request.TechnicianId} does not exist.");
        }

        var hasActiveAssignment = await _db.Assignments
    .AnyAsync(x =>
        x.ServiceRequestId == request.ServiceRequestId &&
        x.IsActive);

if (!AssignmentRules.CanAssignTechnician(
        technician.IsActive,
        hasActiveAssignment))
{
    if (!technician.IsActive)
    {
        return BadRequest(
            $"Technician with ID {request.TechnicianId} is inactive.");
    }

    return Conflict(
        $"Service request {request.ServiceRequestId} already has an active assignment.");
}

        var assignment = new Assignment
        {
            ServiceRequestId = request.ServiceRequestId,
            TechnicianId = request.TechnicianId,
            AssignedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        var statusHistory = new StatusHistory
{
    ServiceRequestId = serviceRequest.Id,
    FromStatus = serviceRequest.Status,
    ToStatus = ServiceRequestStatus.Assigned,
    ChangedAtUtc = DateTime.UtcNow,
    ChangedBy = "system"
};

serviceRequest.Status = ServiceRequestStatus.Assigned;

_db.Assignments.Add(assignment);
_db.StatusHistories.Add(statusHistory);

        await _db.SaveChangesAsync();

        var response = new AssignmentResponse
        {
            Id = assignment.Id,
            ServiceRequestId = assignment.ServiceRequestId,
            TechnicianId = assignment.TechnicianId,
            TechnicianName = technician.Name,
            AssignedAtUtc = assignment.AssignedAtUtc,
            IsActive = assignment.IsActive
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = assignment.Id },
            response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AssignmentResponse>> GetById(int id)
    {
        var assignment = await _db.Assignments
            .AsNoTracking()
            .Include(x => x.Technician)
            .Where(x => x.Id == id)
            .Select(x => new AssignmentResponse
            {
                Id = x.Id,
                ServiceRequestId = x.ServiceRequestId,
                TechnicianId = x.TechnicianId,
                TechnicianName = x.Technician.Name,
                AssignedAtUtc = x.AssignedAtUtc,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();

        if (assignment is null)
        {
            return NotFound();
        }

        return Ok(assignment);
    }
}