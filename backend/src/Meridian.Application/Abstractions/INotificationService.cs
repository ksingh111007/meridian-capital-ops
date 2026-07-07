namespace Meridian.Application.Abstractions;

/// <summary>
/// Notification port (next-approver alerts, capital-call notices, overdue/SLA
/// warnings). The default implementation writes an outbox row and logs; a real
/// channel fan-out (email/Teams per admin notification rules) replaces it behind
/// this interface. Notifications are queued after the audited mutation commits —
/// delivery is best-effort, the outbox row is the durable record.
/// </summary>
public interface INotificationService
{
    /// <param name="recipient">A staff role name, or an investor routing key of the form "investor:{id}".</param>
    Task NotifyRoleAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default);
}
