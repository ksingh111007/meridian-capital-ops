namespace Meridian.Application.Abstractions;

/// <summary>
/// Notification port (next-approver alerts, overdue/SLA warnings). The default
/// implementation writes an outbox row and logs; a real channel fan-out
/// (email/Teams per admin notification rules) replaces it behind this interface.
/// </summary>
public interface INotificationService
{
    Task NotifyRoleAsync(string role, string subject, string body, CancellationToken cancellationToken = default);
}
