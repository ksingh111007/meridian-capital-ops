using Meridian.Application.Abstractions;
using Meridian.Application.Common;
using Meridian.Domain;
using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Application.Automation;

/// <summary>
/// Overdue-call detection (BUSINESS_RULES.md § Overdue calls): an allocation not
/// Wired/Confirmed after the call's due date becomes Overdue, surfaces in
/// needs-attention and fires a notification. Invoked by the Quartz sweep job and
/// the on-demand ops endpoint. Default interest / LPA remedies are future work.
/// </summary>
public class OverdueSweepService(IAppDbContext db, IClock clock, IAuditTrail audit, INotificationService notifications)
{
    public async Task<int> SweepAsync(CancellationToken ct = default)
    {
        var calls = await db.CapitalCalls
            .Where(c => c.Status != CallStatus.Completed)
            .ToListAsync(ct);

        var flipped = 0;
        foreach (var call in calls.Where(c => c.DueDate < clock.Today))
        {
            var late = call.Allocations
                .Where(a => a.WireStatus is WireStatus.Pending or WireStatus.Scheduled)
                .ToList();
            if (late.Count == 0)
                continue;

            foreach (var allocation in late)
                allocation.WireStatus = WireStatus.Overdue;
            flipped += late.Count;

            call.AuditEntries.Add(new CallAuditEntry
            {
                Title = $"{late.Count} allocation{(late.Count == 1 ? "" : "s")} marked overdue",
                By = "System",
                At = clock.UtcNow,
                Tone = "red",
            });
            await audit.AppendAsync("System", "Overdue", "red", $"Call {call.Ref}",
                $"{late.Count} allocation{(late.Count == 1 ? "" : "s")} unpaid past {call.DueDate:yyyy-MM-dd}", ct);
            await notifications.NotifyRoleAsync("Ops Manager", $"Overdue wires on Call {call.Ref}",
                $"{Display.Money(late.Sum(a => a.Amount))} unpaid past {call.DueDate:yyyy-MM-dd}", ct);
        }

        await db.SaveChangesAsync(ct);
        return flipped;
    }
}
