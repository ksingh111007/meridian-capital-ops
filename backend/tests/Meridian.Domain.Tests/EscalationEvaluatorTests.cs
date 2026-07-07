using Meridian.Domain;
using Meridian.Domain.Entities;
using Meridian.Domain.Services;
using Xunit;

namespace Meridian.Domain.Tests;

public class EscalationEvaluatorTests
{
    private static readonly List<WorkflowStage> Stages =
    [
        new() { Order = 1, Name = "Operations", ApproverRole = "Ops Analyst" },
        new() { Order = 3, Name = "CIO", ApproverRole = "CIO" },
    ];

    private static EscalationRule AmountRule(bool enabled = true) => new()
    {
        Id = "esc-amount", Kind = EscalationRuleKind.AmountThreshold, Enabled = enabled,
        ThresholdAmount = 20m, RequiredRoles = ["CIO", "Compliance"],
        Effect = "require CIO + Compliance sign-off",
    };

    [Fact]
    public void AmountOverThreshold_InjectsRolesAndGate()
    {
        var call = new CapitalCall { Amount = 25m };
        var applied = EscalationEvaluator.Apply(call, [AmountRule()], Stages);

        Assert.Single(applied);
        Assert.Equal(["CIO", "Compliance"], call.PendingEscalations);
        Assert.Equal(3, call.EscalationGateStage); // last stage owned by an injected role
    }

    [Fact]
    public void AmountAtOrBelowThreshold_DoesNothing()
    {
        var call = new CapitalCall { Amount = 20m };
        Assert.Empty(EscalationEvaluator.Apply(call, [AmountRule()], Stages));
        Assert.Empty(call.PendingEscalations);
        Assert.Null(call.EscalationGateStage);
    }

    [Fact]
    public void DisabledRule_DoesNothing()
    {
        var call = new CapitalCall { Amount = 100m };
        Assert.Empty(EscalationEvaluator.Apply(call, [AmountRule(enabled: false)], Stages));
        Assert.Empty(call.PendingEscalations);
    }
}
