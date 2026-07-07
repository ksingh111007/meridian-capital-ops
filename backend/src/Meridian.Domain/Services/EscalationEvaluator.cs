using Meridian.Domain.Entities;

namespace Meridian.Domain.Services;

/// <summary>
/// Applies enabled escalation rules to a newly created capital call
/// (BUSINESS_RULES.md: amount &gt; $20M → CIO + Compliance sign-off, etc.).
/// Each applied rule injects per-role sign-offs carrying that rule's own gate
/// stage, so multiple rules compose without weakening each other.
/// Only amount-threshold rules are enforceable today (see <see cref="IsEnforceable"/>):
/// cross-fund allocation is not representable while a call binds to one fund, and
/// new-bank-account rules apply to the wire workflow, which is not built yet.
/// The API reports non-enforceable rules as such so admins are not misled.
/// </summary>
public static class EscalationEvaluator
{
    public static bool IsEnforceable(EscalationRule rule) => rule.Kind == EscalationRuleKind.AmountThreshold;

    public static IReadOnlyList<EscalationRule> Apply(
        CapitalCall call, IEnumerable<EscalationRule> rules, IReadOnlyList<WorkflowStage> stages)
    {
        var applied = new List<EscalationRule>();
        foreach (var rule in rules.Where(r => r.Enabled && IsEnforceable(r)))
        {
            if (rule.ThresholdAmount is not { } threshold || call.Amount <= threshold || rule.RequiredRoles.Count == 0)
                continue;

            // The rule's gate: the call may not advance past the last workflow stage
            // owned by one of its injected roles until every sign-off has landed.
            var gate = stages.Where(s => rule.RequiredRoles.Contains(s.ApproverRole))
                .Select(s => (int?)s.Order)
                .Max() ?? call.CurrentStage;

            foreach (var role in rule.RequiredRoles
                         .Where(role => !call.EscalationSignoffs.Any(s => s.RuleId == rule.Id && s.Role == role)))
            {
                call.EscalationSignoffs.Add(new EscalationSignoff { RuleId = rule.Id, Role = role, GateStage = gate });
            }

            applied.Add(rule);
        }

        return applied;
    }
}
