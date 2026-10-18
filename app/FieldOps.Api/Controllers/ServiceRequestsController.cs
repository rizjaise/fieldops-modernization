using FieldOps.Api.Data;
using FieldOps.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FieldOps.Api.Services;
using FieldOps.Api.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace FieldOps.Api.Controllers;

[ApiController]
[Route("api/service-requests")]
public class ServiceRequestsController : ControllerBase
{
    private readonly FieldOpsDbContext _db;
    private readonly IFieldOpsAuthorizationService _authorizationService;
    private readonly IUserIdentityService _userIdentityService;

    public ServiceRequestsController(
    FieldOpsDbContext db,
    IFieldOpsAuthorizationService authorizationService,
    IUserIdentityService userIdentityService)
    {
        _db = db;
        _authorizationService = authorizationService;
        _userIdentityService = userIdentityService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceRequestResponse>>> GetAll(
    CancellationToken cancellationToken)
    {
        var identityObjectId = _userIdentityService.GetIdentityObjectId();

        if (User.IsInRole(FieldOpsPolicies.Customer) ||
            User.IsInRole(FieldOpsPolicies.Technician))
        {
            if (string.IsNullOrWhiteSpace(identityObjectId))
            {
                return Forbid();
            }
        }

        var query = _db.ServiceRequests
            .AsNoTracking()
            .Include(x => x.Customer)
            .AsQueryable();

        if (User.IsInRole(FieldOpsPolicies.Customer))
        {
            query = query.Where(
                x => x.Customer.IdentityObjectId == identityObjectId);
        }
        else if (User.IsInRole(FieldOpsPolicies.Technician))
        {
            query = query.Where(
                x => _db.Assignments.Any(
                    a => a.ServiceRequestId == x.Id &&
                         a.IsActive &&
                         a.Technician.IdentityObjectId == identityObjectId));
        }
        else if (!User.IsInRole(FieldOpsPolicies.Dispatcher) &&
                 !User.IsInRole(FieldOpsPolicies.Administrator))
        {
            return Forbid();
        }

        var requests = await query
            .Select(x => new ServiceRequestResponse
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer.Name,
                Description = x.Description,
                Status = x.Status,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(requests);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceRequestResponse>> GetById(int id)
    {
        var canAccess = await _authorizationService
            .CanAccessServiceRequestAsync(id);

        if (!canAccess)
        {
            return Forbid();
        }

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

    [Authorize(Policy = FieldOpsPolicies.CustomerOrDispatcherOrAdministrator)]
    [HttpPost]
    public async Task<ActionResult<ServiceRequestResponse>> Create(
    CreateServiceRequestRequest request)
    {
        var customer = await _db.Customers
    .FirstOrDefaultAsync(x => x.Id == request.CustomerId);

        if (customer is null)
        {
            return BadRequest(
                $"Customer with ID {request.CustomerId} does not exist.");
        }

        if (User.IsInRole(FieldOpsPolicies.Customer))
        {
            var identityObjectId = _userIdentityService.GetIdentityObjectId();

            if (string.IsNullOrWhiteSpace(identityObjectId))
            {
                return Forbid();
            }

            if (!string.Equals(
                    customer.IdentityObjectId,
                    identityObjectId,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }
        }

        var serviceRequest = new ServiceRequest
        {
            CustomerId = request.CustomerId,
            Description = request.Description.Trim(),
            Status = ServiceRequestStatus.New,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.ServiceRequests.Add(serviceRequest);

        await _db.SaveChangesAsync();

        var response = await _db.ServiceRequests
            .AsNoTracking()
            .Where(x => x.Id == serviceRequest.Id)
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
            new { id = serviceRequest.Id },
            response);
    }

    [Authorize(Policy = FieldOpsPolicies.TechnicianOrDispatcherOrAdministrator)]
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

        if (User.IsInRole(FieldOpsPolicies.Technician))
        {
            var canAccess = await _authorizationService
                .CanAccessServiceRequestAsync(id);

            if (!canAccess)
            {
                return Forbid();
            }
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