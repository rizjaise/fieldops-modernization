using FieldOps.Api.Data;
using FieldOps.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FieldOps.Api.Services;

namespace FieldOps.Api.Controllers;

[ApiController]
[Route("api/service-requests")]
public class ServiceRequestsController : ControllerBase
{
    private readonly FieldOpsDbContext _db;

    public ServiceRequestsController(FieldOpsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceRequestResponse>>> GetAll()
    {
        var requests = await _db.ServiceRequests
            .AsNoTracking()
            .Include(x => x.Customer)
            .Select(x => new ServiceRequestResponse
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer.Name,
                Description = x.Description,
                Status = x.Status,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(requests);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceRequestResponse>> GetById(int id)
    {
        var request = await _db.ServiceRequests
            .AsNoTracking()
            .Include(x => x.Customer)
            .Where(x => x.Id == id)
            .Select(x => new ServiceRequestResponse
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer.Name,
                Description = x.Description,
                Status = x.Status,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .FirstOrDefaultAsync();

        if (request is null)
        {
            return NotFound();
        }

        return Ok(request);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceRequestResponse>> Create(
        ServiceRequest request)
    {
        var customerExists = await _db.Customers
            .AnyAsync(x => x.Id == request.CustomerId);

        if (!customerExists)
        {
            return BadRequest(
                $"Customer with ID {request.CustomerId} does not exist.");
        }

        request.Id = 0;
        request.Status = "New";
        request.CreatedAtUtc = DateTime.UtcNow;

        _db.ServiceRequests.Add(request);

        await _db.SaveChangesAsync();

        var response = await _db.ServiceRequests
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new ServiceRequestResponse
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer.Name,
                Description = x.Description,
                Status = x.Status,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .FirstAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = request.Id },
            response);
    }

    [HttpPatch("{id:int}/status")]
public async Task<ActionResult<ServiceRequestResponse>> UpdateStatus(
    int id,
    UpdateServiceRequestStatusRequest request)
{
    var serviceRequest = await _db.ServiceRequests
        .Include(x => x.Customer)
        .FirstOrDefaultAsync(x => x.Id == id);

    if (serviceRequest is null)
    {
        return NotFound();
    }

    var requestedStatus = request.Status.Trim();

    if (!ServiceRequestStatus.All.Contains(requestedStatus))
    {
        return BadRequest(
            $"Invalid status '{request.Status}'.");
    }

    var currentStatus = serviceRequest.Status;

    if (currentStatus.Equals(
            requestedStatus,
            StringComparison.OrdinalIgnoreCase))
    {
        return BadRequest(
            $"Service request is already '{currentStatus}'.");
    }

    var validTransition =
        currentStatus == ServiceRequestStatus.New &&
        requestedStatus == ServiceRequestStatus.Assigned
        ||
        currentStatus == ServiceRequestStatus.Assigned &&
        requestedStatus == ServiceRequestStatus.InProgress
        ||
        currentStatus == ServiceRequestStatus.InProgress &&
        requestedStatus == ServiceRequestStatus.Completed;

    if (!ServiceRequestStatusRules.IsValidTransition(
        currentStatus,
        requestedStatus))
{
    return BadRequest(
        $"Cannot transition service request from '{currentStatus}' to '{requestedStatus}'.");
}

    var history = new StatusHistory
    {
        ServiceRequestId = serviceRequest.Id,
        FromStatus = currentStatus,
        ToStatus = requestedStatus,
        ChangedAtUtc = DateTime.UtcNow,
        ChangedBy = "system"
    };

    serviceRequest.Status = requestedStatus;

    _db.StatusHistories.Add(history);

    await _db.SaveChangesAsync();

    var response = new ServiceRequestResponse
    {
        Id = serviceRequest.Id,
        CustomerId = serviceRequest.CustomerId,
        CustomerName = serviceRequest.Customer.Name,
        Description = serviceRequest.Description,
        Status = serviceRequest.Status,
        CreatedAtUtc = serviceRequest.CreatedAtUtc
    };

    return Ok(response);
}
}