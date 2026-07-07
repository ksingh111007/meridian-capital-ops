using Meridian.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Application.Workflows;

public sealed record WorkflowStageDto(
    int Order, string Name, string ApproverRole, int? SlaDays, bool AutoAdvance, bool Required, bool Terminal);

public sealed record EscalationRuleDto(string Condition, string Effect, bool Enabled);

public sealed record WorkflowDto(
    string WorkflowName, IReadOnlyList<WorkflowStageDto> Stages, IReadOnlyList<EscalationRuleDto> EscalationRules);

public class WorkflowService(IAppDbContext db)
{
    public async Task<WorkflowDto> GetCapitalCallWorkflowAsync(CancellationToken ct = default)
    {
        var stages = await db.WorkflowStages.AsNoTracking().OrderBy(s => s.Order).ToListAsync(ct);
        var rules = await db.EscalationRules.AsNoTracking().OrderBy(r => r.Id).ToListAsync(ct);
        return new WorkflowDto(
            "Capital Calls",
            stages.Select(s => new WorkflowStageDto(s.Order, s.Name, s.ApproverRole, s.SlaDays, s.AutoAdvance, s.Required, s.Terminal)).ToList(),
            rules.Select(r => new EscalationRuleDto(r.Condition, r.Effect, r.Enabled)).ToList());
    }
}
