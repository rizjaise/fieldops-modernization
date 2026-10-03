namespace FieldOps.Api.Services;

public static class AssignmentRules
{
    public static bool CanAssignTechnician(
        bool technicianIsActive,
        bool hasActiveAssignment)
    {
        return technicianIsActive && !hasActiveAssignment;
    }
}