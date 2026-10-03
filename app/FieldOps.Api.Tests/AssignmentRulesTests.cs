using FieldOps.Api.Services;

namespace FieldOps.Api.Tests;

public class AssignmentRulesTests
{
    [Fact]
    public void ActiveTechnician_WithNoActiveAssignment_IsAllowed()
    {
        var result = AssignmentRules.CanAssignTechnician(
            technicianIsActive: true,
            hasActiveAssignment: false);

        Assert.True(result);
    }

    [Fact]
    public void InactiveTechnician_CannotBeAssigned()
    {
        var result = AssignmentRules.CanAssignTechnician(
            technicianIsActive: false,
            hasActiveAssignment: false);

        Assert.False(result);
    }

    [Fact]
    public void ExistingActiveAssignment_PreventsAnotherAssignment()
    {
        var result = AssignmentRules.CanAssignTechnician(
            technicianIsActive: true,
            hasActiveAssignment: true);

        Assert.False(result);
    }

    [Fact]
    public void InactiveTechnician_WithExistingAssignment_IsRejected()
    {
        var result = AssignmentRules.CanAssignTechnician(
            technicianIsActive: false,
            hasActiveAssignment: true);

        Assert.False(result);
    }
}