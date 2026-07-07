namespace Meridian.Application.Abstractions;

public sealed record AuditEntry(string Actor, string Action, string Tone, string Subject, string Detail);

/// <summary>
/// Appends to the global, hash-chained audit log. Every mutation must go through
/// this — no exceptions (docs/API.md cross-cutting requirements).
///
/// Transactional contract: appending flushes the scoped unit of work, committing
/// the pending business mutation and its audit event(s) atomically. Callers must
/// therefore NOT SaveChanges separately before appending — stage the mutation on
/// the tracked entities, then append; a failure before the append persists nothing,
/// and a mutation can never commit without its audit record.
/// </summary>
public interface IAuditTrail
{
    Task AppendAsync(string actor, string action, string tone, string subject, string detail,
        CancellationToken cancellationToken = default);

    /// <summary>Seals and commits several events (plus the pending unit of work) in one transaction.</summary>
    Task AppendAllAsync(IReadOnlyList<AuditEntry> entries, CancellationToken cancellationToken = default);
}
