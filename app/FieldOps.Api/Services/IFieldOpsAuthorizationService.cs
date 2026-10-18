namespace FieldOps.Api.Services;

public interface IFieldOpsAuthorizationService
{
    Task<bool> CanAccessServiceRequestAsync(
        int serviceRequestId,
        CancellationToken cancellationToken = default);

    Task<bool> CanAccessAssignmentAsync(
        int assignmentId,
        CancellationToken cancellationToken = default);
}