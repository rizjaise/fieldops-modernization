using FieldOps.Api.Models;
using FieldOps.Api.Services;

namespace FieldOps.Api.Tests;

public class ServiceRequestStatusRulesTests
{
    [Fact]
    public void New_To_Assigned_Is_Valid()
    {
        var result = ServiceRequestStatusRules.IsValidTransition(
            ServiceRequestStatus.New,
            ServiceRequestStatus.Assigned);

        Assert.True(result);
    }

    [Fact]
    public void Assigned_To_InProgress_Is_Valid()
    {
        var result = ServiceRequestStatusRules.IsValidTransition(
            ServiceRequestStatus.Assigned,
            ServiceRequestStatus.InProgress);

        Assert.True(result);
    }

    [Fact]
    public void InProgress_To_Completed_Is_Valid()
    {
        var result = ServiceRequestStatusRules.IsValidTransition(
            ServiceRequestStatus.InProgress,
            ServiceRequestStatus.Completed);

        Assert.True(result);
    }

    [Fact]
    public void New_To_Completed_Is_Invalid()
    {
        var result = ServiceRequestStatusRules.IsValidTransition(
            ServiceRequestStatus.New,
            ServiceRequestStatus.Completed);

        Assert.False(result);
    }

    [Fact]
    public void Completed_To_InProgress_Is_Invalid()
    {
        var result = ServiceRequestStatusRules.IsValidTransition(
            ServiceRequestStatus.Completed,
            ServiceRequestStatus.InProgress);

        Assert.False(result);
    }
}