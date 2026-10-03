namespace FieldOps.Api.Models;

public static class ServiceRequestStatus
{
    public const string New = "New";
    public const string Assigned = "Assigned";
    public const string InProgress = "In Progress";
    public const string Completed = "Completed";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            New,
            Assigned,
            InProgress,
            Completed
        };
}