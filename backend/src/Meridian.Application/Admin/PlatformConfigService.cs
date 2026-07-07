using Meridian.Application.Abstractions;
using Meridian.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Application.Admin;

public sealed record IntegrationKpisDto(int Connected, int Warnings, int Errors, string LastSync, string LastSyncAgo);

public sealed record IntegrationDto(
    string Name, string Type, string Direction, string LastSync, string Status, string? Warning);

public sealed record IntegrationsDto(IntegrationKpisDto Kpis, IReadOnlyList<IntegrationDto> Integrations);

public sealed record NotificationRuleDto(
    string Id, string Name, string Trigger, string Channel, string Recipients, bool Enabled);

public sealed record NotificationChannelDto(string Name, string Detail, bool Connected);

public sealed record NotificationRulesDto(
    IReadOnlyList<NotificationRuleDto> Rules, IReadOnlyList<NotificationChannelDto> Channels);

public sealed record AccessKpisDto(
    int PortalUsers, int Active, int PendingInvites, string InvestorsWithAccess, int NotEnrolled);

public sealed record PortalContactDto(
    string Id, string Name, string Initials, string InvestorId, string Investor, string Role,
    string FundsVisible, string Statements, string Status);

public sealed record LabelToggleDto(string Label, bool Enabled);

public sealed record ExposedTypeDto(string Label, bool Exposed);

public sealed record InvestorAccessDto(
    AccessKpisDto Kpis, IReadOnlyList<PortalContactDto> Contacts, IReadOnlyList<LabelToggleDto> Capabilities,
    IReadOnlyList<ExposedTypeDto> DocumentTypes);

public class PlatformConfigService(IAppDbContext db)
{
    public async Task<IntegrationsDto> GetIntegrationsAsync(CancellationToken ct = default)
    {
        var kpis = await KpiReader.ForScreenAsync(db, "integrations", ct);
        var integrations = await db.Integrations.AsNoTracking().OrderBy(i => i.Name).ToListAsync(ct);

        return new IntegrationsDto(
            new IntegrationKpisDto(
                kpis.Count("connected"), kpis.Count("warnings"), kpis.Count("errors"),
                kpis.Text("lastSync"), kpis.Text("lastSyncAgo")),
            integrations.Select(i => new IntegrationDto(
                i.Name, i.Type, i.Direction, i.LastSync, i.Status, i.Warning)).ToList());
    }

    public async Task<NotificationRulesDto> GetNotificationRulesAsync(CancellationToken ct = default)
    {
        var rules = await db.NotificationRules.AsNoTracking().OrderBy(r => r.Id).ToListAsync(ct);
        var channels = await db.NotificationChannels.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

        return new NotificationRulesDto(
            rules.Select(r => new NotificationRuleDto(r.Id, r.Name, r.Trigger, r.Channel, r.Recipients, r.Enabled)).ToList(),
            channels.Select(c => new NotificationChannelDto(c.Name, c.Detail, c.Connected)).ToList());
    }

    public async Task<InvestorAccessDto> GetInvestorAccessAsync(CancellationToken ct = default)
    {
        var kpis = await KpiReader.ForScreenAsync(db, "investor-access", ct);
        var contacts = await db.PortalContacts.AsNoTracking().OrderBy(c => c.Id).ToListAsync(ct);
        var capabilities = await db.PortalCapabilities.AsNoTracking().OrderBy(c => c.SortOrder).ToListAsync(ct);
        var documentTypes = await db.PortalDocumentTypes.AsNoTracking().OrderBy(t => t.SortOrder).ToListAsync(ct);

        return new InvestorAccessDto(
            new AccessKpisDto(
                kpis.Count("portalUsers"), kpis.Count("active"), kpis.Count("pendingInvites"),
                kpis.Text("investorsWithAccess"), kpis.Count("notEnrolled")),
            contacts.Select(c => new PortalContactDto(
                c.Id, c.Name, c.Initials, c.InvestorId, c.InvestorName, c.Role, c.FundsVisible,
                c.Statements, c.Status)).ToList(),
            capabilities.Select(c => new LabelToggleDto(c.Label, c.Enabled)).ToList(),
            documentTypes.Select(t => new ExposedTypeDto(t.Label, t.Exposed)).ToList());
    }
}
