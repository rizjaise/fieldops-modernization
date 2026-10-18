namespace FieldOps.Api.Authorization;

public static class FieldOpsPolicies
{
    public const string Customer = "Customer";
    public const string Technician = "Technician";
    public const string Dispatcher = "Dispatcher";
    public const string Administrator = "Administrator";

    public const string CustomerOrDispatcherOrAdministrator =
        "CustomerOrDispatcherOrAdministrator";

    public const string TechnicianOrDispatcherOrAdministrator =
        "TechnicianOrDispatcherOrAdministrator";

    public const string DispatcherOrAdministrator =
        "DispatcherOrAdministrator";
}