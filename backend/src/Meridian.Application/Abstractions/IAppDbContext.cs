using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Application.Abstractions;

/// <summary>
/// The persistence port. Implemented by EF Core in Meridian.Infrastructure;
/// today it is backed by an in-memory SQLite database, later by the dedicated
/// database project without touching this layer.
/// </summary>
public interface IAppDbContext
{
    DbSet<Fund> Funds { get; }
    DbSet<Deal> Deals { get; }
    DbSet<Investor> Investors { get; }
    DbSet<CapitalCall> CapitalCalls { get; }
    DbSet<Distribution> Distributions { get; }
    DbSet<WorkflowStage> WorkflowStages { get; }
    DbSet<EscalationRule> EscalationRules { get; }
    DbSet<StaffUser> StaffUsers { get; }
    DbSet<Role> Roles { get; }
    DbSet<AuditEvent> AuditEvents { get; }
    DbSet<Notification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
