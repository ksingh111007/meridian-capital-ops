using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Application.Abstractions;

/// <summary>
/// The persistence port. Implemented by EF Core in Meridian.Infrastructure;
/// backed by Azure SQL (schema owned by the database/ dacpac project) when
/// Database:Provider = SqlServer, or by in-memory SQLite for dev/tests.
/// </summary>
public interface IAppDbContext
{
    DbSet<Fund> Funds { get; }
    DbSet<LegalEntity> LegalEntities { get; }
    DbSet<ShareClass> ShareClasses { get; }
    DbSet<Deal> Deals { get; }
    DbSet<DealDetail> DealDetails { get; }
    DbSet<Investor> Investors { get; }
    DbSet<InvestorProfile> InvestorProfiles { get; }
    DbSet<Borrower> Borrowers { get; }
    DbSet<CurrencyRate> CurrencyRates { get; }
    DbSet<SettlementCalendar> SettlementCalendars { get; }
    DbSet<CapitalCall> CapitalCalls { get; }
    DbSet<Distribution> Distributions { get; }
    DbSet<WorkflowStage> WorkflowStages { get; }
    DbSet<EscalationRule> EscalationRules { get; }
    DbSet<Drawdown> Drawdowns { get; }
    DbSet<Wire> Wires { get; }
    DbSet<ReconItem> ReconItems { get; }
    DbSet<CashAccount> CashAccounts { get; }
    DbSet<CashPositionSnapshot> CashPositionSnapshots { get; }
    DbSet<PortfolioSnapshot> PortfolioSnapshots { get; }
    DbSet<KpiSnapshot> KpiSnapshots { get; }
    DbSet<StaffUser> StaffUsers { get; }
    DbSet<Role> Roles { get; }
    DbSet<Integration> Integrations { get; }
    DbSet<NotificationRule> NotificationRules { get; }
    DbSet<NotificationChannel> NotificationChannels { get; }
    DbSet<AuditEvent> AuditEvents { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<PortalContact> PortalContacts { get; }
    DbSet<PortalCapability> PortalCapabilities { get; }
    DbSet<PortalDocumentType> PortalDocumentTypes { get; }
    DbSet<PortalFundPosition> PortalFundPositions { get; }
    DbSet<PortalAccountSnapshot> PortalAccountSnapshots { get; }
    DbSet<PortalRollforwardLine> PortalRollforwardLines { get; }
    DbSet<PortalActivityRow> PortalActivityRows { get; }
    DbSet<PortalDocument> PortalDocuments { get; }
    DbSet<PortalTaxDocument> PortalTaxDocuments { get; }
    DbSet<PortalIrConfig> PortalIrConfigs { get; }
    DbSet<PortalIrRegardingOption> PortalIrRegardingOptions { get; }
    DbSet<PortalIrRequest> PortalIrRequests { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
