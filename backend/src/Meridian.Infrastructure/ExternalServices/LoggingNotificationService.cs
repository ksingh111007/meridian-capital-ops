using Meridian.Application.Abstractions;
using Meridian.Domain.Entities;
using Meridian.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace Meridian.Infrastructure.ExternalServices;

/// <summary>
/// Default notification adapter: writes an outbox row (queryable, testable) and
/// logs. The production adapter fans out to channels per the admin
/// notification-rules config behind the same port.
/// </summary>
public sealed class LoggingNotificationService(AppDbContext db, IClock clock, ILogger<LoggingNotificationService> logger)
    : INotificationService
{
    public async Task NotifyRoleAsync(string role, string subject, string body, CancellationToken cancellationToken = default)
    {
        db.Notifications.Add(new Notification { At = clock.UtcNow, RecipientRole = role, Subject = subject, Body = body });
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Notify [{Role}] {Subject}: {Body}", role, subject, body);
    }
}
