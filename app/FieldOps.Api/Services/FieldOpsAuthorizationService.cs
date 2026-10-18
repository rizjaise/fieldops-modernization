using System.Security.Claims;
using FieldOps.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Services;

public class FieldOpsAuthorizationService : IFieldOpsAuthorizationService
{
    private readonly FieldOpsDbContext _db;
    private readonly IUserIdentityService _userIdentityService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FieldOpsAuthorizationService(
        FieldOpsDbContext db,
        IUserIdentityService userIdentityService,
        IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _userIdentityService = userIdentityService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> CanAccessServiceRequestAsync(
        int serviceRequestId,
        CancellationToken cancellationToken = default)
    {
        if (IsPrivilegedUser())
        {
            return await _db.ServiceRequests
                .AnyAsync(
                    x => x.Id == serviceRequestId,
                    cancellationToken);
        }

        var identityObjectId =
            _userIdentityService.GetIdentityObjectId();

        if (string.IsNullOrWhiteSpace(identityObjectId))
        {
            return false;
        }

        var customerOwnsRequest = await _db.ServiceRequests
            .AnyAsync(
                x =>
                    x.Id == serviceRequestId &&
                    x.Customer.IdentityObjectId == identityObjectId,
                cancellationToken);

        if (customerOwnsRequest)
        {
            return true;
        }

        return await _db.Assignments
            .AnyAsync(
                x =>
                    x.ServiceRequestId == serviceRequestId &&
                    x.IsActive &&
                    x.Technician.IdentityObjectId == identityObjectId,
                cancellationToken);
    }

    public async Task<bool> CanAccessAssignmentAsync(
        int assignmentId,
        CancellationToken cancellationToken = default)
    {
        if (IsPrivilegedUser())
        {
            return await _db.Assignments
                .AnyAsync(
                    x => x.Id == assignmentId,
                    cancellationToken);
        }

        var identityObjectId =
            _userIdentityService.GetIdentityObjectId();

        if (string.IsNullOrWhiteSpace(identityObjectId))
        {
            return false;
        }

        return await _db.Assignments
            .AnyAsync(
                x =>
                    x.Id == assignmentId &&
                    x.Technician.IdentityObjectId == identityObjectId,
                cancellationToken);
    }

    private bool IsPrivilegedUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        return user?.IsInRole("Dispatcher") == true ||
               user?.IsInRole("Administrator") == true;
    }
}