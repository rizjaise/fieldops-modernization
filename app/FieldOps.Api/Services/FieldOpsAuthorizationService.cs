using System.Security.Claims;
using FieldOps.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Services;

public class FieldOpsAuthorizationService
    : IFieldOpsAuthorizationService
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
        var user = _httpContextAccessor.HttpContext?.User;

        if (user is null)
        {
            return false;
        }

        if (user.IsInRole("Dispatcher") ||
            user.IsInRole("Administrator"))
        {
            return await _db.ServiceRequests
                .AsNoTracking()
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

        if (user.IsInRole("Customer"))
        {
            return await _db.ServiceRequests
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.Id == serviceRequestId &&
                        x.Customer.IdentityObjectId ==
                            identityObjectId,
                    cancellationToken);
        }

        if (user.IsInRole("Technician"))
        {
            return await _db.Assignments
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.ServiceRequestId == serviceRequestId &&
                        x.IsActive &&
                        x.Technician.IdentityObjectId ==
                            identityObjectId,
                    cancellationToken);
        }

        return false;
    }

    public async Task<bool> CanAccessAssignmentAsync(
        int assignmentId,
        CancellationToken cancellationToken = default)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user is null)
        {
            return false;
        }

        if (user.IsInRole("Dispatcher") ||
            user.IsInRole("Administrator"))
        {
            return await _db.Assignments
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == assignmentId,
                    cancellationToken);
        }

        if (!user.IsInRole("Technician"))
        {
            return false;
        }

        var identityObjectId =
            _userIdentityService.GetIdentityObjectId();

        if (string.IsNullOrWhiteSpace(identityObjectId))
        {
            return false;
        }

        return await _db.Assignments
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == assignmentId &&
                    x.IsActive &&
                    x.Technician.IdentityObjectId ==
                        identityObjectId,
                cancellationToken);
    }
}