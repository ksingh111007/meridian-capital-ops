using Meridian.Application.Abstractions;
using Meridian.Domain.Entities;
using Meridian.Domain.Services;
using Meridian.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Infrastructure.Services;

/// <summary>
/// Hash-chained global audit log. Appends are serialized through a process-wide
/// gate so each seal links to the true previous event, and the single save inside
/// the gate flushes the scoped unit of work — committing the business mutation and
/// its audit events together (see the IAuditTrail transactional contract).
/// </summary>
public sealed class AuditTrail(AppDbContext db, IClock clock) : IAuditTrail
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    public Task AppendAsync(string actor, string action, string tone, string subject, string detail,
        CancellationToken cancellationToken = default) =>
        AppendAllAsync([new AuditEntry(actor, action, tone, subject, detail)], cancellationToken);

    public async Task AppendAllAsync(IReadOnlyList<AuditEntry> entries, CancellationToken cancellationToken = default)
    {
        if (entries.Count == 0)
            return;

        await Gate.WaitAsync(cancellationToken);
        try
        {
            var previousSeal = (await db.AuditEvents.AsNoTracking()
                .OrderByDescending(e => e.Id)
                .FirstOrDefaultAsync(cancellationToken))?.Seal;
            var at = clock.UtcNow;

            foreach (var entry in entries)
            {
                var seal = AuditSealer.ComputeSeal(previousSeal, at, entry.Actor, entry.Action, entry.Subject, entry.Detail);
                db.AuditEvents.Add(new AuditEvent
                {
                    At = at,
                    Actor = entry.Actor,
                    Action = entry.Action,
                    Tone = entry.Tone,
                    Subject = entry.Subject,
                    Detail = entry.Detail,
                    Seal = seal,
                });
                previousSeal = seal;
            }

            await db.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            Gate.Release();
        }
    }
}
