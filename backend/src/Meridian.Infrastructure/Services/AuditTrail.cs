using Meridian.Application.Abstractions;
using Meridian.Domain.Entities;
using Meridian.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Meridian.Infrastructure.Persistence;

namespace Meridian.Infrastructure.Services;

/// <summary>
/// Hash-chained global audit log. Appends are serialized through a process-wide
/// gate so each seal links to the true previous event; the save also flushes the
/// scoped unit of work, committing the mutation and its audit entry together.
/// </summary>
public sealed class AuditTrail(AppDbContext db, IClock clock) : IAuditTrail
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    public async Task AppendAsync(string actor, string action, string tone, string subject, string detail,
        CancellationToken cancellationToken = default)
    {
        await Gate.WaitAsync(cancellationToken);
        try
        {
            var previous = await db.AuditEvents.AsNoTracking()
                .OrderByDescending(e => e.Id)
                .FirstOrDefaultAsync(cancellationToken);
            var at = clock.UtcNow;
            db.AuditEvents.Add(new AuditEvent
            {
                At = at,
                Actor = actor,
                Action = action,
                Tone = tone,
                Subject = subject,
                Detail = detail,
                Seal = AuditSealer.ComputeSeal(previous?.Seal, at, actor, action, subject, detail),
            });
            await db.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            Gate.Release();
        }
    }
}
