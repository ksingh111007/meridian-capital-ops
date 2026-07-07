using Meridian.Domain.Entities;

namespace Meridian.Domain.Services;

/// <summary>
/// Applies enabled escalation rules to a newly created capital call
/// (BUSINESS_RULES.md: amount &gt; $20M → CIO + Compliance sign-off, etc.).
/// Only amount-threshold rules are evaluable today: cross-fund allocation is not
/// representable while a call binds to one fund, and new-bank-account rules apply
/// to the wire workflow, which is not built yet.
/// </summary>
public static class EscalationEvaluator
{
    public static IReadOnlyList<EscalationRule> Apply(
        CapitalCall call, IEnumerable<EscalationRule> rules, IReadOnlyList<WorkflowStage> stages)
    {
        var applied = new List<EscalationRule>();
        foreach (var rule in rules.Where(r => r.Enabled && r.Kind == EscalationRuleKind.AmountThreshold))
        {
            if (rule.ThresholdAmount is not { } threshold || call.Amount <= threshold || rule.RequiredRoles.Count == 0)
                continue;

            foreach (var role in rule.RequiredRoles.Where(role => !call.PendingEscalations.Contains(role)))
                call.PendingEscalations.Add(role);

            // The call may not advance past the last workflow stage owned by one of
            // the injected roles until every sign-off has landed.
            var gate = stages.Where(s => rule.RequiredRoles.Contains(s.ApproverRole))
                .Select(s => (int?)s.Order)
                .Max() ?? call.CurrentStage;
            call.EscalationGateStage = Math.Max(call.EscalationGateStage ?? 0, gate);
            applied.Add(rule);
        }

        return applied;
    }
}
