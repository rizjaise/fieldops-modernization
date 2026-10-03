using FieldOps.Api.Models;

namespace FieldOps.Api.Services;

public static class ServiceRequestStatusRules
{
    public static bool IsValidTransition(
        string currentStatus,
        string requestedStatus)
    {
        return currentStatus == ServiceRequestStatus.New &&
               requestedStatus == ServiceRequestStatus.Assigned
            || currentStatus == ServiceRequestStatus.Assigned &&
               requestedStatus == ServiceRequestStatus.InProgress
            || currentStatus == ServiceRequestStatus.InProgress &&
               requestedStatus == ServiceRequestStatus.Completed;
    }
}