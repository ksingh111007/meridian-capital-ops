using System.Text.Json;
using Meridian.Application.Abstractions;
using Meridian.Domain;
using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Meridian.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    private readonly bool _isSqlServer;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        _isSqlServer = options.Extensions.Any(e => e.GetType().FullName!.Contains("SqlServer"));
    }

    public DbSet<Fund> Funds => Set<Fund>();
    public DbSet<LegalEntity> LegalEntities => Set<LegalEntity>();
    public DbSet<ShareClass> ShareClasses => Set<ShareClass>();
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<DealDetail> DealDetails => Set<DealDetail>();
    public DbSet<Investor> Investors => Set<Investor>();
    public DbSet<InvestorProfile> InvestorProfiles => Set<InvestorProfile>();
    public DbSet<Borrower> Borrowers => Set<Borrower>();
    public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();
    public DbSet<SettlementCalendar> SettlementCalendars => Set<SettlementCalendar>();
    public DbSet<CapitalCall> CapitalCalls => Set<CapitalCall>();
    public DbSet<Distribution> Distributions => Set<Distribution>();
    public DbSet<WorkflowStage> WorkflowStages => Set<WorkflowStage>();
    public DbSet<EscalationRule> EscalationRules => Set<EscalationRule>();
    public DbSet<Drawdown> Drawdowns => Set<Drawdown>();
    public DbSet<Wire> Wires => Set<Wire>();
    public DbSet<ReconItem> ReconItems => Set<ReconItem>();
    public DbSet<CashAccount> CashAccounts => Set<CashAccount>();
    public DbSet<CashPositionSnapshot> CashPositionSnapshots => Set<CashPositionSnapshot>();
    public DbSet<PortfolioSnapshot> PortfolioSnapshots => Set<PortfolioSnapshot>();
    public DbSet<KpiSnapshot> KpiSnapshots => Set<KpiSnapshot>();
    public DbSet<StaffUser> StaffUsers => Set<StaffUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Integration> Integrations => Set<Integration>();
    public DbSet<NotificationRule> NotificationRules => Set<NotificationRule>();
    public DbSet<NotificationChannel> NotificationChannels => Set<NotificationChannel>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PortalContact> PortalContacts => Set<PortalContact>();
    public DbSet<PortalCapability> PortalCapabilities => Set<PortalCapability>();
    public DbSet<PortalDocumentType> PortalDocumentTypes => Set<PortalDocumentType>();
    public DbSet<PortalFundPosition> PortalFundPositions => Set<PortalFundPosition>();
    public DbSet<PortalAccountSnapshot> PortalAccountSnapshots => Set<PortalAccountSnapshot>();
    public DbSet<PortalRollforwardLine> PortalRollforwardLines => Set<PortalRollforwardLine>();
    public DbSet<PortalActivityRow> PortalActivityRows => Set<PortalActivityRow>();
    public DbSet<PortalDocument> PortalDocuments => Set<PortalDocument>();
    public DbSet<PortalTaxDocument> PortalTaxDocuments => Set<PortalTaxDocument>();
    public DbSet<PortalIrConfig> PortalIrConfigs => Set<PortalIrConfig>();
    public DbSet<PortalIrRegardingOption> PortalIrRegardingOptions => Set<PortalIrRegardingOption>();
    public DbSet<PortalIrRequest> PortalIrRequests => Set<PortalIrRequest>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        if (_isSqlServer)
        {
            // Azure SQL stores native decimals; amounts are 2-dp USD millions.
            // Columns needing more scale (FX rates, KPI metrics) override below.
            configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        }
        else
        {
            // Dev store is SQLite, which has no native decimal: persist as REAL so both
            // EF and Dapper can aggregate. Amounts are 2-dp USD millions, so double is
            // lossless here; the database project maps native decimal.
            configurationBuilder.Properties<decimal>().HaveConversion<double>();
        }

        // Bounded strings so the SQL Server schema avoids nvarchar(max) by default;
        // JSON-bag columns override to max length explicitly.
        configurationBuilder.Properties<string>().HaveMaxLength(400);

        // Enums as readable strings so hand-written Dapper SQL stays legible.
        configurationBuilder.Properties<WaterfallType>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<FundStatus>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<CallStatus>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<WireStatus>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<StageState>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<DistributionStatus>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<PayoutStatus>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<AllocationBasis>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<EscalationRuleKind>().HaveConversion<string>().HaveMaxLength(40);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---------- ref: master & reference data ----------

        modelBuilder.Entity<Fund>(e =>
        {
            e.ToTable("Funds", DbSchemas.Ref);
            e.HasKey(f => f.Id);
        });

        modelBuilder.Entity<LegalEntity>(e =>
        {
            e.ToTable("LegalEntities", DbSchemas.Ref);
            e.HasKey(x => x.Id);
            e.HasOne<Fund>().WithMany().HasForeignKey(x => x.FundId);
        });

        modelBuilder.Entity<ShareClass>(e =>
        {
            e.ToTable("ShareClasses", DbSchemas.Ref);
            e.HasKey(x => x.Id);
            e.HasOne<Fund>().WithMany().HasForeignKey(x => x.FundId);
        });

        modelBuilder.Entity<Deal>(e =>
        {
            e.ToTable("Deals", DbSchemas.Ref);
            e.HasKey(d => d.Id);
        });

        modelBuilder.Entity<DealDetail>(e =>
        {
            e.ToTable("DealDetails", DbSchemas.Ref);
            e.HasKey(d => d.DealId);
            e.HasOne<Deal>().WithOne().HasForeignKey<DealDetail>(d => d.DealId);
            e.OwnsMany(d => d.Cashflows, c => Owned(c, "DealCashflows", DbSchemas.Ref));
            e.OwnsMany(d => d.LpExposures, c => Owned(c, "DealLpExposures", DbSchemas.Ref));
            e.OwnsMany(d => d.Documents, c => Owned(c, "DealDocuments", DbSchemas.Ref));
        });

        modelBuilder.Entity<Investor>(e =>
        {
            e.ToTable("Investors", DbSchemas.Ref);
            e.HasKey(i => i.Id);
            e.OwnsMany(i => i.Commitments, c => Owned(c, "InvestorCommitments", DbSchemas.Ref));
        });

        modelBuilder.Entity<InvestorProfile>(e =>
        {
            e.ToTable("InvestorProfiles", DbSchemas.Ref);
            e.HasKey(p => p.InvestorId);
            e.HasOne<Investor>().WithOne().HasForeignKey<InvestorProfile>(p => p.InvestorId);
        });

        modelBuilder.Entity<Borrower>(e =>
        {
            e.ToTable("Borrowers", DbSchemas.Ref);
            e.HasKey(b => b.Name);
        });

        modelBuilder.Entity<CurrencyRate>(e =>
        {
            e.ToTable("CurrencyRates", DbSchemas.Ref);
            e.HasKey(c => c.Code);
            e.Property(c => c.Code).HasMaxLength(3);
            e.Property(c => c.Rate).HasPrecision(18, 6);
        });

        modelBuilder.Entity<SettlementCalendar>(e =>
        {
            e.ToTable("SettlementCalendars", DbSchemas.Ref);
            e.HasKey(c => c.Name);
        });

        // ---------- ops: fund operations ----------

        modelBuilder.Entity<CapitalCall>(e =>
        {
            e.ToTable("CapitalCalls", DbSchemas.Ops);
            e.HasKey(c => c.Id);
            e.OwnsMany(c => c.Allocations, a => Owned(a, "CallAllocations", DbSchemas.Ops));
            e.OwnsMany(c => c.StageEvents, a =>
            {
                Owned(a, "CallStageEvents", DbSchemas.Ops);
                a.Property(s => s.Comment).HasMaxLength(2000); // free text — approver comments
            });
            e.OwnsMany(c => c.Documents, a => Owned(a, "CallDocuments", DbSchemas.Ops));
            e.OwnsMany(c => c.AuditEntries, a =>
            {
                Owned(a, "CallAuditEntries", DbSchemas.Ops);
                a.Property(x => x.Comment).HasMaxLength(2000); // free text — approver comments
            });
            e.Property(c => c.PendingEscalations)
                .HasMaxLength(2000)
                .HasConversion(StringListConverter, StringListComparer);
        });

        modelBuilder.Entity<Distribution>(e =>
        {
            e.ToTable("Distributions", DbSchemas.Ops);
            e.HasKey(d => d.Id);
            e.OwnsMany(d => d.Tiers, t => Owned(t, "DistributionTiers", DbSchemas.Ops));
            e.OwnsMany(d => d.Payouts, p => Owned(p, "DistributionPayouts", DbSchemas.Ops));
        });

        modelBuilder.Entity<WorkflowStage>(e =>
        {
            e.ToTable("WorkflowStages", DbSchemas.Ops);
            e.HasKey(s => s.Order);
            e.Property(s => s.Order).ValueGeneratedNever();
        });

        modelBuilder.Entity<EscalationRule>(e =>
        {
            e.ToTable("EscalationRules", DbSchemas.Ops);
            e.HasKey(r => r.Id);
            e.Property(r => r.RequiredRoles)
                .HasMaxLength(2000)
                .HasConversion(StringListConverter, StringListComparer);
        });

        modelBuilder.Entity<Drawdown>(e =>
        {
            e.ToTable("Drawdowns", DbSchemas.Ops);
            e.HasKey(d => d.Id);
        });

        modelBuilder.Entity<Wire>(e =>
        {
            e.ToTable("Wires", DbSchemas.Ops);
            e.HasKey(w => w.Id);
            e.HasIndex(w => w.Status);
        });

        modelBuilder.Entity<ReconItem>(e =>
        {
            e.ToTable("ReconItems", DbSchemas.Ops);
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.Status);
        });

        modelBuilder.Entity<CashAccount>(e =>
        {
            e.ToTable("CashAccounts", DbSchemas.Ops);
            e.HasKey(a => a.Id);
        });

        modelBuilder.Entity<CashPositionSnapshot>(e =>
        {
            e.ToTable("CashPositionSnapshots", DbSchemas.Ops);
            e.HasKey(s => s.Id);
            e.OwnsMany(s => s.ForecastBars, o => Owned(o, "CashForecastBars", DbSchemas.Ops));
            e.OwnsMany(s => s.Weeks, o => Owned(o, "CashForecastWeeks", DbSchemas.Ops));
        });

        modelBuilder.Entity<PortfolioSnapshot>(e =>
        {
            e.ToTable("PortfolioSnapshots", DbSchemas.Ops);
            e.HasKey(s => s.Id);
            e.OwnsMany(s => s.ValueTrend, o => Owned(o, "PortfolioTrendPoints", DbSchemas.Ops));
        });

        modelBuilder.Entity<KpiSnapshot>(e =>
        {
            e.ToTable("KpiSnapshots", DbSchemas.Ops);
            e.HasKey(k => k.Id);
            e.Property(k => k.ScreenKey).HasMaxLength(60);
            e.Property(k => k.MetricKey).HasMaxLength(60);
            e.Property(k => k.NumericValue).HasPrecision(18, 4);
            e.HasIndex(k => new { k.ScreenKey, k.MetricKey }).IsUnique();
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.ToTable("Notifications", DbSchemas.Ops);
            e.HasKey(n => n.Id);
            e.Property(n => n.Body).HasMaxLength(2000);
        });

        // ---------- admin: staff & platform configuration ----------

        modelBuilder.Entity<StaffUser>(e =>
        {
            e.ToTable("StaffUsers", DbSchemas.Admin);
            e.HasKey(u => u.Id);
        });

        modelBuilder.Entity<Role>(e =>
        {
            e.ToTable("Roles", DbSchemas.Admin);
            e.HasKey(r => r.Name);
            e.Property(r => r.Capabilities).HasMaxLength(2000).HasConversion(
                new ValueConverter<Dictionary<ModuleName, Capability>, string>(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<ModuleName, Capability>>(v, (JsonSerializerOptions?)null)
                         ?? new Dictionary<ModuleName, Capability>()),
                new ValueComparer<Dictionary<ModuleName, Capability>>(
                    (a, b) => a!.OrderBy(x => x.Key).SequenceEqual(b!.OrderBy(x => x.Key)),
                    v => v.Aggregate(0, (h, kv) => HashCode.Combine(h, kv.Key, kv.Value)),
                    v => new Dictionary<ModuleName, Capability>(v)));
        });

        modelBuilder.Entity<Integration>(e =>
        {
            e.ToTable("Integrations", DbSchemas.Admin);
            e.HasKey(i => i.Name);
        });

        modelBuilder.Entity<NotificationRule>(e =>
        {
            e.ToTable("NotificationRules", DbSchemas.Admin);
            e.HasKey(r => r.Id);
        });

        modelBuilder.Entity<NotificationChannel>(e =>
        {
            e.ToTable("NotificationChannels", DbSchemas.Admin);
            e.HasKey(c => c.Name);
        });

        // ---------- audit: append-only hash-chained log ----------

        modelBuilder.Entity<AuditEvent>(e =>
        {
            e.ToTable("Events", DbSchemas.Audit);
            e.HasKey(a => a.Id);
            e.Property(a => a.Detail).HasMaxLength(2000); // carries quoted approver comments
            e.Property(a => a.Seal).HasMaxLength(12);
            e.Property(a => a.PreviousSeal).HasMaxLength(12);
            // One successor per seal: concurrent appends / retried commits cannot
            // silently fork the hash chain (see AuditEvent.PreviousSeal).
            e.HasIndex(a => a.PreviousSeal).IsUnique()
                .HasFilter(_isSqlServer ? "[PreviousSeal] IS NOT NULL" : "\"PreviousSeal\" IS NOT NULL");
        });

        // ---------- portal: LP identities, capital accounts, documents ----------

        modelBuilder.Entity<PortalContact>(e =>
        {
            e.ToTable("Contacts", DbSchemas.Portal);
            e.HasKey(c => c.Id);
            e.HasOne<Investor>().WithMany().HasForeignKey(c => c.InvestorId);
        });

        modelBuilder.Entity<PortalCapability>(e =>
        {
            e.ToTable("Capabilities", DbSchemas.Portal);
            e.HasKey(c => c.Label);
        });

        modelBuilder.Entity<PortalDocumentType>(e =>
        {
            e.ToTable("DocumentTypes", DbSchemas.Portal);
            e.HasKey(t => t.Label);
        });

        modelBuilder.Entity<PortalFundPosition>(e =>
        {
            e.ToTable("FundPositions", DbSchemas.Portal);
            e.HasKey(p => p.Id);
            e.HasOne<Investor>().WithMany().HasForeignKey(p => p.InvestorId);
            e.HasOne<Fund>().WithMany().HasForeignKey(p => p.FundId);
            e.HasIndex(p => new { p.InvestorId, p.FundId }).IsUnique();
        });

        modelBuilder.Entity<PortalAccountSnapshot>(e =>
        {
            e.ToTable("AccountSnapshots", DbSchemas.Portal);
            e.HasKey(s => s.InvestorId);
            e.HasOne<Investor>().WithOne().HasForeignKey<PortalAccountSnapshot>(s => s.InvestorId);
        });

        modelBuilder.Entity<PortalRollforwardLine>(e =>
        {
            e.ToTable("RollforwardLines", DbSchemas.Portal);
            e.HasKey(l => l.Id);
            e.HasOne<Investor>().WithMany().HasForeignKey(l => l.InvestorId);
            e.OwnsMany(l => l.Amounts, o => Owned(o, "RollforwardAmounts", DbSchemas.Portal));
        });

        modelBuilder.Entity<PortalActivityRow>(e =>
        {
            e.ToTable("ActivityRows", DbSchemas.Portal);
            e.HasKey(r => r.Id);
            e.HasOne<Investor>().WithMany().HasForeignKey(r => r.InvestorId);
            e.HasIndex(r => new { r.InvestorId, r.Date });
        });

        modelBuilder.Entity<PortalDocument>(e =>
        {
            e.ToTable("Documents", DbSchemas.Portal);
            e.HasKey(d => d.Id);
            e.HasOne<Investor>().WithMany().HasForeignKey(d => d.InvestorId);
        });

        modelBuilder.Entity<PortalTaxDocument>(e =>
        {
            e.ToTable("TaxDocuments", DbSchemas.Portal);
            e.HasKey(d => d.Id);
            e.HasOne<Investor>().WithMany().HasForeignKey(d => d.InvestorId);
        });

        modelBuilder.Entity<PortalIrConfig>(e =>
        {
            e.ToTable("IrConfig", DbSchemas.Portal);
            e.HasKey(c => c.Id);
        });

        modelBuilder.Entity<PortalIrRegardingOption>(e =>
        {
            e.ToTable("IrRegardingOptions", DbSchemas.Portal);
            e.HasKey(o => o.Label);
        });

        modelBuilder.Entity<PortalIrRequest>(e =>
        {
            e.ToTable("IrRequests", DbSchemas.Portal);
            e.HasKey(r => r.Id);
            e.HasOne<Investor>().WithMany().HasForeignKey(r => r.InvestorId);
            e.Property(r => r.Message).HasMaxLength(2000); // free text — the LP's message body
        });

        AddAuditColumns(modelBuilder);
    }

    /// <summary>
    /// Every table (owned collections included) carries the platform audit columns
    /// and an active flag, added as shadow state so domain entities stay clean.
    /// <see cref="AuditColumnsInterceptor"/> stamps them on save; SQL defaults cover
    /// rows inserted outside EF (the dacpac post-deployment seed).
    /// </summary>
    private void AddAuditColumns(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Entity ids are short slugs ("call-2041"); keep key/FK columns narrow.
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string) && property.Name.EndsWith("Id"))
                    property.SetMaxLength(64);
            }

            var isActive = entityType.AddProperty(AuditColumns.IsActive, typeof(bool));
            isActive.SetDefaultValue(true);

            var createdAt = entityType.AddProperty(AuditColumns.CreatedAtUtc, typeof(DateTime));
            createdAt.SetDefaultValueSql(_isSqlServer ? "SYSUTCDATETIME()" : "CURRENT_TIMESTAMP");

            var createdBy = entityType.AddProperty(AuditColumns.CreatedBy, typeof(string));
            createdBy.SetMaxLength(100);
            createdBy.SetDefaultValue("system");
            createdBy.IsNullable = false;

            var modifiedAt = entityType.AddProperty(AuditColumns.ModifiedAtUtc, typeof(DateTime?));
            modifiedAt.IsNullable = true;

            var modifiedBy = entityType.AddProperty(AuditColumns.ModifiedBy, typeof(string));
            modifiedBy.SetMaxLength(100);
            modifiedBy.IsNullable = true;
        }
    }

    /// <summary>
    /// Owned-collection tables get their own auto-increment Id as the sole primary
    /// key — the default composite (OwnerId, shadow Id) key cannot be value-generated
    /// by SQLite.
    /// </summary>
    private static void Owned<TOwner, TDependent>(
        OwnedNavigationBuilder<TOwner, TDependent> builder, string table, string schema)
        where TOwner : class
        where TDependent : class
    {
        builder.ToTable(table, schema);
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
