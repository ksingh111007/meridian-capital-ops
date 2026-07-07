using Meridian.Application.Abstractions;
using Meridian.Domain;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Application.Automation;

/// <summary>
/// Detects approval stages breaching their SLA and fires the "Approval overdue"
/// notification (BUSINESS_RULES.md § Due-diligence approval pipeline). Scaffolding:
/// notifications re-fire on every run — production adds a dedupe/acknowledge store.
/// </summary>
public class ApprovalSlaService(IAppDbContext db, IClock clock, INotificationService notifications)
{
    public async Task<int> CheckAsync(CancellationToken ct = default)
    {
        var stages = await db.WorkflowStages.AsNoTracking().ToDictionaryAsync(s => s.Order, ct);
        var openCalls = await db.CapitalCalls.AsNoTracking()
            .Where(c => c.Status != CallStatus.Completed)
            .ToListAsync(ct);

        var breaches = 0;
        foreach (var call in openCalls)
        {
            if (!stages.TryGetValue(call.CurrentStage, out var stage) || stage.SlaDays is not { } sla)
                continue;
            var enteredOn = call.StageEvents.FirstOrDefault(e => e.Stage == call.CurrentStage)?.Date;
            if (enteredOn is not { } entered || entered.AddDays(sla) >= clock.Today)
                continue;

            breaches++;
            await notifications.NotifyRoleAsync(stage.ApproverRole,
                $"Approval overdue — Call {call.Ref} at {stage.Name}",
                $"In stage since {entered:yyyy-MM-dd}; SLA is {sla}d.", ct);
        }

        return breaches;
    }
}
