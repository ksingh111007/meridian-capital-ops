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
        // Wire status is independent of pipeline status: a Completed call can still
        // carry unpaid allocations, so the filter is the due date, never Call.Status.
        var today = clock.Today;
        var pastDue = await db.CapitalCalls
            .Where(c => c.DueDate < today)
            .ToListAsync(ct);

        var flipped = 0;
        var auditEntries = new List<AuditEntry>();
        var alerts = new List<(string Subject, string Body)>();
        foreach (var call in pastDue)
        {
            var late = call.Allocations
                .Where(a => a.WireStatus is WireStatus.Pending or WireStatus.Scheduled)
                .ToList();
            if (late.Count == 0)
                continue;

            foreach (var allocation in late)
                allocation.WireStatus = WireStatus.Overdue;
            flipped += late.Count;
            call.Version++;

            var noun = $"{late.Count} allocation{(late.Count == 1 ? "" : "s")}";
            call.AuditEntries.Add(new CallAuditEntry
            {
                Title = $"{noun} marked overdue",
                By = "System",
                At = clock.UtcNow,
                Tone = "red",
            });
            auditEntries.Add(new AuditEntry("System", "Overdue", "red", $"Call {call.Ref}",
                $"{noun} unpaid past {call.DueDate:yyyy-MM-dd}"));
            alerts.Add(($"Overdue wires on Call {call.Ref}",
                $"{Display.Money(late.Sum(a => a.Amount))} unpaid past {call.DueDate:yyyy-MM-dd}"));
        }

        // Single atomic commit for every flip and its audit event — a mid-sweep
        // failure persists nothing, and re-running never double-audits.
        await audit.AppendAllAsync(auditEntries, ct);
        foreach (var (subject, body) in alerts)
            await notifications.NotifyRoleAsync("Ops Manager", subject, body, ct);

        return flipped;
    }
}
