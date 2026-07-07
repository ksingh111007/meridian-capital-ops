using System.Text.Json;
using Meridian.Application.Abstractions;
using Meridian.Domain;
using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Meridian.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Fund> Funds => Set<Fund>();
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<Investor> Investors => Set<Investor>();
    public DbSet<CapitalCall> CapitalCalls => Set<CapitalCall>();
    public DbSet<Distribution> Distributions => Set<Distribution>();
    public DbSet<WorkflowStage> WorkflowStages => Set<WorkflowStage>();
    public DbSet<EscalationRule> EscalationRules => Set<EscalationRule>();
    public DbSet<StaffUser> StaffUsers => Set<StaffUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Dev store is SQLite, which has no native decimal: persist as REAL so both
        // EF and Dapper can aggregate. Amounts are 2-dp USD millions, so double is
        // lossless here; the production database project maps native decimal.
        configurationBuilder.Properties<decimal>().HaveConversion<double>();

        // Enums as readable strings so hand-written Dapper SQL stays legible.
        configurationBuilder.Properties<WaterfallType>().HaveConversion<string>();
        configurationBuilder.Properties<FundStatus>().HaveConversion<string>();
        configurationBuilder.Properties<CallStatus>().HaveConversion<string>();
        configurationBuilder.Properties<WireStatus>().HaveConversion<string>();
        configurationBuilder.Properties<StageState>().HaveConversion<string>();
        configurationBuilder.Properties<DistributionStatus>().HaveConversion<string>();
        configurationBuilder.Properties<PayoutStatus>().HaveConversion<string>();
        configurationBuilder.Properties<AllocationBasis>().HaveConversion<string>();
        configurationBuilder.Properties<EscalationRuleKind>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fund>().HasKey(f => f.Id);
        modelBuilder.Entity<Deal>().HasKey(d => d.Id);

        modelBuilder.Entity<Investor>(e =>
        {
            e.HasKey(i => i.Id);
            e.OwnsMany(i => i.Commitments, c => Owned(c, "InvestorCommitments"));
        });

        modelBuilder.Entity<CapitalCall>(e =>
        {
            e.HasKey(c => c.Id);
            e.OwnsMany(c => c.Allocations, a => Owned(a, "CallAllocations"));
            e.OwnsMany(c => c.StageEvents, a => Owned(a, "CallStageEvents"));
            e.OwnsMany(c => c.Documents, a => Owned(a, "CallDocuments"));
            e.OwnsMany(c => c.AuditEntries, a => Owned(a, "CallAuditEntries"));
            e.Property(c => c.PendingEscalations).HasConversion(StringListConverter, StringListComparer);
        });

        modelBuilder.Entity<Distribution>(e =>
        {
            e.HasKey(d => d.Id);
            e.OwnsMany(d => d.Tiers, t => Owned(t, "DistributionTiers"));
            e.OwnsMany(d => d.Payouts, p => Owned(p, "DistributionPayouts"));
        });

        modelBuilder.Entity<WorkflowStage>(e =>
        {
            e.HasKey(s => s.Order);
            e.Property(s => s.Order).ValueGeneratedNever();
        });

        modelBuilder.Entity<EscalationRule>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.RequiredRoles).HasConversion(StringListConverter, StringListComparer);
        });

        modelBuilder.Entity<StaffUser>().HasKey(u => u.Id);

        modelBuilder.Entity<Role>(e =>
        {
            e.HasKey(r => r.Name);
            e.Property(r => r.Capabilities).HasConversion(
                new ValueConverter<Dictionary<ModuleName, Capability>, string>(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<ModuleName, Capability>>(v, (JsonSerializerOptions?)null)
                         ?? new Dictionary<ModuleName, Capability>()),
                new ValueComparer<Dictionary<ModuleName, Capability>>(
                    (a, b) => a!.OrderBy(x => x.Key).SequenceEqual(b!.OrderBy(x => x.Key)),
                    v => v.Aggregate(0, (h, kv) => HashCode.Combine(h, kv.Key, kv.Value)),
                    v => new Dictionary<ModuleName, Capability>(v)));
        });

        modelBuilder.Entity<AuditEvent>().HasKey(a => a.Id);
        modelBuilder.Entity<Notification>().HasKey(n => n.Id);
    }

    /// <summary>
    /// Owned-collection tables get their own auto-increment Id as the sole primary
    /// key — the default composite (OwnerId, shadow Id) key cannot be value-generated
    /// by SQLite.
    /// </summary>
    private static void Owned<TOwner, TDependent>(OwnedNavigationBuilder<TOwner, TDependent> builder, string table)
        where TOwner : class
        where TDependent : class
    {
        builder.ToTable(table);
        builder.Property<long>("Id").ValueGeneratedOnAdd();
        builder.HasKey("Id");
    }

    private static readonly ValueConverter<List<string>, string> StringListConverter = new(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

    private static readonly ValueComparer<List<string>> StringListComparer = new(
        (a, b) => a!.SequenceEqual(b!),
        v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
        v => v.ToList());
}
