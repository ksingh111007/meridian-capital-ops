using Meridian.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Meridian.Infrastructure.Persistence;

/// <summary>Shadow audit-column names shared by the model and the database project.</summary>
public static class AuditColumns
{
    public const string IsActive = "IsActive";
    public const string CreatedAtUtc = "CreatedAtUtc";
    public const string CreatedBy = "CreatedBy";
    public const string ModifiedAtUtc = "ModifiedAtUtc";
    public const string ModifiedBy = "ModifiedBy";
}

/// <summary>
/// Stamps the shadow audit columns on every insert/update so row-level provenance
/// never depends on callers remembering to set it.
/// </summary>
public sealed class AuditColumnsInterceptor(IClock clock, IAuditActorProvider actorProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null)
            return;

        var now = clock.UtcNow;
        var actor = actorProvider.CurrentActor;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Metadata.FindProperty(AuditColumns.CreatedAtUtc) is null)
                continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(AuditColumns.CreatedAtUtc).CurrentValue = now;
                    entry.Property(AuditColumns.CreatedBy).CurrentValue = actor;
                    entry.Property(AuditColumns.IsActive).CurrentValue = true;
                    break;
                case EntityState.Modified:
                    entry.Property(AuditColumns.ModifiedAtUtc).CurrentValue = now;
                    entry.Property(AuditColumns.ModifiedBy).CurrentValue = actor;
                    break;
            }
        }
    }
}

/// <summary>Fallback actor for hosts without a request context (jobs, seeding, tests).</summary>
public sealed class SystemAuditActorProvider : IAuditActorProvider
{
    public string CurrentActor => "system";
}
