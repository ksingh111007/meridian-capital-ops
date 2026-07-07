namespace Meridian.Application.Abstractions;

/// <summary>
/// Appends to the global, hash-chained audit log. Every mutation must call this —
/// no exceptions (docs/API.md cross-cutting requirements).
/// </summary>
public interface IAuditTrail
{
    Task AppendAsync(string actor, string action, string tone, string subject, string detail,
        CancellationToken cancellationToken = default);
}
