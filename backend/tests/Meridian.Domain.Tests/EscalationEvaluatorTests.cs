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
        new() { Order = 4, Name = "Legal", ApproverRole = "Counsel" },
    ];

    private static EscalationRule AmountRule(bool enabled = true) => new()
    {
        Id = "esc-amount", Kind = EscalationRuleKind.AmountThreshold, Enabled = enabled,
        ThresholdAmount = 20m, RequiredRoles = ["CIO", "Compliance"],
        Effect = "require CIO + Compliance sign-off",
    };

    [Fact]
    public void AmountOverThreshold_InjectsPerRoleSignoffsWithTheRulesGate()
    {
        var call = new CapitalCall { Amount = 25m };
        var applied = EscalationEvaluator.Apply(call, [AmountRule()], Stages);

        Assert.Single(applied);
        Assert.Equal(["CIO", "Compliance"], call.EscalationSignoffs.Select(s => s.Role));
        // Gate = last stage owned by one of the rule's roles (CIO, stage 3).
        Assert.All(call.EscalationSignoffs, s => Assert.Equal(3, s.GateStage));
    }

    [Fact]
    public void AmountAtOrBelowThreshold_DoesNothing()
    {
        var call = new CapitalCall { Amount = 20m };
        Assert.Empty(EscalationEvaluator.Apply(call, [AmountRule()], Stages));
        Assert.Empty(call.EscalationSignoffs);
    }

    [Fact]
    public void DisabledRule_DoesNothing()
    {
        var call = new CapitalCall { Amount = 100m };
        Assert.Empty(EscalationEvaluator.Apply(call, [AmountRule(enabled: false)], Stages));
        Assert.Empty(call.EscalationSignoffs);
    }

    [Fact]
    public void TwoRules_KeepTheirOwnGates_NoMerging()
    {
        var legalRule = new EscalationRule
        {
            Id = "esc-legal", Kind = EscalationRuleKind.AmountThreshold, Enabled = true,
            ThresholdAmount = 50m, RequiredRoles = ["Counsel"], Effect = "require Legal sign-off",
        };
        var call = new CapitalCall { Amount = 60m };

        var applied = EscalationEvaluator.Apply(call, [AmountRule(), legalRule], Stages);

        Assert.Equal(2, applied.Count);
        Assert.Equal(3, call.EscalationSignoffs.Single(s => s.Role == "CIO").GateStage);
        Assert.Equal(4, call.EscalationSignoffs.Single(s => s.Role == "Counsel").GateStage);
    }

    [Fact]
    public void OnlyAmountThresholdRules_AreEnforceable()
    {
        Assert.True(EscalationEvaluator.IsEnforceable(AmountRule()));
        Assert.False(EscalationEvaluator.IsEnforceable(new EscalationRule { Kind = EscalationRuleKind.CrossFundAllocation }));
        Assert.False(EscalationEvaluator.IsEnforceable(new EscalationRule { Kind = EscalationRuleKind.NewBankAccount }));
    }
}
