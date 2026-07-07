using Meridian.Domain;
using Meridian.Domain.Entities;
using Meridian.Domain.Services;
using Xunit;

namespace Meridian.Domain.Tests;

public class ApprovalWorkflowTests
{
    private static readonly DateOnly Today = new(2026, 7, 5);
    private static readonly DateTime Now = new(2026, 7, 5, 12, 0, 0, DateTimeKind.Utc);

    private static List<WorkflowStage> Stages() =>
    [
        new() { Order = 1, Name = "Operations", ApproverRole = "Ops Analyst", SlaDays = 1, Required = true },
        new() { Order = 2, Name = "Front Office", ApproverRole = "Deal Lead", SlaDays = 1, Required = true },
        new() { Order = 3, Name = "CIO", ApproverRole = "CIO", SlaDays = 2, Required = true },
        new() { Order = 4, Name = "Legal", ApproverRole = "Counsel", SlaDays = 2, Required = true },
        new() { Order = 5, Name = "Ops Final Review", ApproverRole = "Ops Manager", SlaDays = 1, Required = true },
        new() { Order = 6, Name = "Accounting", ApproverRole = "Fund Accountant", SlaDays = 1, Required = true },
        new() { Order = 7, Name = "Book", ApproverRole = "System", AutoAdvance = true, Required = true },
        new() { Order = 8, Name = "Custodians Notified", ApproverRole = "System", AutoAdvance = true },
        new() { Order = 9, Name = "Completed", ApproverRole = "", Terminal = true },
    ];

    private static CapitalCall Call(int stage = 1) => new()
    {
        Id = "call-1", Ref = "#C-1", Amount = 10m, CurrentStage = stage, Status = CallStatus.InReview,
    };

    [Fact]
    public void Approve_AdvancesOneStage_AndRecordsStageEvent()
    {
        var call = Call(stage: 4);
        var outcome = ApprovalWorkflow.Approve(call, Stages(),
            new Actor("J. Okafor", "Counsel", false), "LPA §4.2 reviewed", Today, Now);

        Assert.Equal(5, call.CurrentStage);
        Assert.Equal(CallStatus.InReview, call.Status);
        Assert.False(outcome.Completed);
        Assert.Equal("Ops Manager", outcome.NextApproverRole);
        var done = Assert.Single(call.StageEvents, e => e.Stage == 4);
        Assert.Equal(StageState.Done, done.State);
        Assert.Equal("LPA §4.2 reviewed", done.Comment);
        Assert.Single(call.StageEvents, e => e.Stage == 5 && e.State == StageState.Current);
    }

    [Fact]
    public void Approve_AtLastManualStage_SkipsAutoStagesAndCompletes()
    {
        var call = Call(stage: 6);
        var outcome = ApprovalWorkflow.Approve(call, Stages(),
            new Actor("Dana Whitfield", "Fund Accountant", false), "Booked", Today, Now);

        Assert.True(outcome.Completed);
        Assert.Equal(9, call.CurrentStage);
        Assert.Equal(CallStatus.Completed, call.Status);
        Assert.Equal(StageState.Done, call.StageEvents.Single(e => e.Stage == 7).State);
        Assert.Equal(StageState.Done, call.StageEvents.Single(e => e.Stage == 8).State);
    }

    [Fact]
    public void Approve_RequiresComment()
    {
        var ex = Assert.Throws<DomainException>(() => ApprovalWorkflow.Approve(
            Call(), Stages(), new Actor("Jordan Chen", "Ops Analyst", false), "  ", Today, Now));
        Assert.Equal(ErrorKind.Validation, ex.Kind);
    }

    [Fact]
    public void Approve_WrongRole_IsForbidden()
    {
        var ex = Assert.Throws<DomainException>(() => ApprovalWorkflow.Approve(
            Call(stage: 4), Stages(), new Actor("Jordan Chen", "Ops Analyst", false), "ok", Today, Now));
        Assert.Equal(ErrorKind.Forbidden, ex.Kind);
    }

    [Fact]
    public void Approve_FullApprovalsCapability_MayActOnAnyStage()
    {
        var call = Call(stage: 4);
        ApprovalWorkflow.Approve(call, Stages(), new Actor("Avery Whitman", "Administrator", true), "override", Today, Now);
        Assert.Equal(5, call.CurrentStage);
    }

    [Fact]
    public void Approve_CompletedCall_Conflicts()
    {
        var call = Call(stage: 9);
        call.Status = CallStatus.Completed;
        var ex = Assert.Throws<DomainException>(() => ApprovalWorkflow.Approve(
            call, Stages(), new Actor("Avery Whitman", "Administrator", true), "late", Today, Now));
        Assert.Equal(ErrorKind.Conflict, ex.Kind);
    }

    [Fact]
    public void Reject_ReturnsToPriorStage()
    {
        var call = Call(stage: 4);
        var prior = ApprovalWorkflow.Reject(call, Stages(),
            new Actor("J. Okafor", "Counsel", false), "Allocation edited", Today, Now);

        Assert.Equal("CIO", prior);
        Assert.Equal(3, call.CurrentStage);
        Assert.Equal(CallStatus.Returned, call.Status);
        Assert.Single(call.StageEvents, e => e.Stage == 3 && e.State == StageState.Current);
        Assert.DoesNotContain(call.StageEvents, e => e.Stage == 4);
    }

    [Fact]
    public void Reject_AtFirstStage_Conflicts()
    {
        var ex = Assert.Throws<DomainException>(() => ApprovalWorkflow.Reject(
            Call(stage: 1), Stages(), new Actor("Jordan Chen", "Ops Analyst", false), "no", Today, Now));
        Assert.Equal(ErrorKind.Conflict, ex.Kind);
    }

    [Fact]
    public void Escalation_SignoffClears_WithoutAdvancingStage()
    {
        var call = Call(stage: 3);
        call.PendingEscalations = ["CIO", "Compliance"];
        call.EscalationGateStage = 3;

        var outcome = ApprovalWorkflow.Approve(call, Stages(),
            new Actor("Priya Nair", "Compliance", false), "Reviewed", Today, Now);

        Assert.True(outcome.EscalationSignoffOnly);
        Assert.Equal(3, call.CurrentStage);
        Assert.Equal(["CIO"], call.PendingEscalations);
    }

    [Fact]
    public void Escalation_GateBlocksStageApproval_UntilAllSignoffsLand()
    {
        var call = Call(stage: 3);
        call.PendingEscalations = ["CIO", "Compliance"];
        call.EscalationGateStage = 3;

        // CIO owns stage 3 and clears their own escalation entry, but Compliance is outstanding.
        var ex = Assert.Throws<DomainException>(() => ApprovalWorkflow.Approve(
            call, Stages(), new Actor("Sanjay Patel", "CIO", false), "Proceed", Today, Now));
        Assert.Equal(ErrorKind.Conflict, ex.Kind);
        Assert.Contains("Compliance", ex.Message);

        // After the Compliance sign-off the CIO approval advances normally.
        ApprovalWorkflow.Approve(call, Stages(), new Actor("Priya Nair", "Compliance", false), "Reviewed", Today, Now);
        var outcome = ApprovalWorkflow.Approve(call, Stages(), new Actor("Sanjay Patel", "CIO", false), "Proceed", Today, Now);
        Assert.False(outcome.EscalationSignoffOnly);
        Assert.Equal(4, call.CurrentStage);
        Assert.Empty(call.PendingEscalations);
    }
}
