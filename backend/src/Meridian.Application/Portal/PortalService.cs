using Meridian.Application.Abstractions;
using Meridian.Application.Common;
using Meridian.Domain;
using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Application.Portal;

public sealed record PortalStatsDto(decimal Commitment, decimal PaidIn, decimal Distributions, decimal Nav, decimal NetIrrPct);

public sealed record PortalFundCardDto(
    string FundId, string Name, int Vintage, decimal Commitment, decimal Nav, decimal NetIrrPct,
    decimal Dpi, decimal CalledPct, decimal CalledAmount);

public sealed record PortalAccountDto(
    string InvestorId, string Investor, string ContactName, string ContactInitials, string AsOf,
    PortalStatsDto Stats, IReadOnlyList<PortalFundCardDto> Funds);

public sealed record PortalPositionDto(
    string Fund, int Vintage, decimal Commitment, decimal PaidIn, decimal Distributions, decimal Nav,
    decimal NetIrrPct, decimal Tvpi);

public sealed record PortalTotalsDto(
    decimal Commitment, decimal PaidIn, decimal Distributions, decimal Nav, decimal NetIrrPct, decimal Tvpi);

/// <summary>Pivoted to the frontend's fundIII/fundII columns (the LP's two funds).</summary>
public sealed record RollforwardLineDto(string Line, decimal? FundIII, decimal? FundII, decimal? Total, string Kind);

public sealed record RollforwardDto(string Period, IReadOnlyList<RollforwardLineDto> Lines);

public sealed record PortalInvestmentsDto(
    string AsOf, IReadOnlyList<PortalPositionDto> Positions, PortalTotalsDto Totals, RollforwardDto Rollforward);

public sealed record PortalActivityStatsDto(
    decimal PaidIn, decimal DistributionsReceived, decimal NetInvested, string? NextCallDue);

public sealed record PortalActivityRowDto(
    string Date, string Fund, string Type, string Reference, decimal Amount, string Status);

public sealed record PortalActivityDto(PortalActivityStatsDto Stats, IReadOnlyList<PortalActivityRowDto> Rows);

public sealed record PortalDocumentDto(string Id, string Name, string Fund, string Period, string Type, string Date);

public sealed record PortalStatementsDto(int TotalCount, IReadOnlyList<PortalDocumentDto> Documents);

public sealed record TaxBannerDto(string Headline, string Detail);

public sealed record PortalTaxDocumentDto(
    string Id, string Name, string Fund, int TaxYear, string Type, string Status, string? ExpectedDate);

public sealed record PortalTaxDto(TaxBannerDto Banner, IReadOnlyList<PortalTaxDocumentDto> Documents);

public sealed record IrManagerDto(string Name, string Initials, string Title);

public sealed record IrRequestDto(string Subject, string Ref, string Date, string Status);

public sealed record PortalIrInfoDto(
    IrManagerDto Manager, string Email, string Phone, string Hours, IReadOnlyList<string> RegardingOptions,
    IReadOnlyList<IrRequestDto> RecentRequests);

public sealed record CreateIrMessageRequest(string Subject, string Regarding, string Message);

/// <summary>The portal shell's identity strip — available to every portal contact.</summary>
public sealed record PortalSessionDto(
    string ContactId, string ContactName, string ContactInitials, string InvestorId, string Investor, string Role);

/// <summary>
/// Investor-portal reads/mutations, always scoped to the session's LP — investor
/// ids never come from request parameters. Tax-only contacts see only tax
/// documents and the IR desk (BACKEND_TODO portal acceptance criteria).
/// </summary>
public class PortalService(IAppDbContext db, IPortalSessionProvider sessionProvider, IAuditTrail audit, IClock clock)
{
    /// <summary>
    /// Who is signed in — allowed for every portal contact (including Tax-only,
    /// who cannot read the capital account) so the shell can always render.
    /// </summary>
    public async Task<PortalSessionDto> GetSessionAsync(CancellationToken ct = default)
    {
        var session = await sessionProvider.GetRequiredAsync(ct);
        var investor = await GetInvestorAsync(session.InvestorId, ct);
        return new PortalSessionDto(
            session.ContactId, session.ContactName, session.ContactInitials,
            investor.Id, investor.Name, session.Role);
    }

    public async Task<PortalAccountDto> GetAccountAsync(CancellationToken ct = default)
    {
        var session = await RequireCapitalAccountAccessAsync(ct);
        var investor = await GetInvestorAsync(session.InvestorId, ct);
        var snapshot = await GetSnapshotAsync(session.InvestorId, ct);
        var positions = await PositionsAsync(session.InvestorId, ct);

        return new PortalAccountDto(
            investor.Id, investor.Name, session.ContactName, session.ContactInitials,
            snapshot.AsOf.ToString("yyyy-MM-dd"),
            new PortalStatsDto(snapshot.Commitment, snapshot.PaidIn, snapshot.Distributions, snapshot.Nav, snapshot.NetIrrPct),
            positions.Select(p => new PortalFundCardDto(
                p.FundId, p.FundName, p.Vintage, p.Commitment, p.Nav, p.NetIrrPct, p.Dpi,
                p.CalledPct, p.CalledAmount)).ToList());
    }

    public async Task<PortalInvestmentsDto> GetInvestmentsAsync(CancellationToken ct = default)
    {
        var session = await RequireCapitalAccountAccessAsync(ct);
        var snapshot = await GetSnapshotAsync(session.InvestorId, ct);
        var positions = await PositionsAsync(session.InvestorId, ct);
        var lines = await db.PortalRollforwardLines.AsNoTracking()
            .Where(l => l.InvestorId == session.InvestorId)
            .OrderBy(l => l.SortOrder)
            .ToListAsync(ct);

        return new PortalInvestmentsDto(
            snapshot.AsOf.ToString("yyyy-MM-dd"),
            positions.Select(p => new PortalPositionDto(
                p.FundName, p.Vintage, p.Commitment, p.PaidIn, p.Distributions, p.Nav, p.NetIrrPct, p.Tvpi)).ToList(),
            new PortalTotalsDto(
                snapshot.Commitment, snapshot.PaidIn, snapshot.Distributions, snapshot.Nav,
                snapshot.NetIrrPct, snapshot.Tvpi),
            new RollforwardDto(
                lines.FirstOrDefault()?.Period ?? "",
                lines.Select(l => new RollforwardLineDto(
                    l.Label,
                    l.Amounts.FirstOrDefault(a => a.FundId == "fund-iii")?.Amount,
                    l.Amounts.FirstOrDefault(a => a.FundId == "fund-ii")?.Amount,
                    l.Total, l.Kind)).ToList()));
    }

    public async Task<PortalActivityDto> GetActivityAsync(CancellationToken ct = default)
    {
        var session = await RequireCapitalAccountAccessAsync(ct);
        var snapshot = await GetSnapshotAsync(session.InvestorId, ct);
        var rows = await db.PortalActivityRows.AsNoTracking()
            .Where(r => r.InvestorId == session.InvestorId)
            .OrderByDescending(r => r.Date)
            .ToListAsync(ct);

        return new PortalActivityDto(
            new PortalActivityStatsDto(
                snapshot.PaidIn, snapshot.Distributions, snapshot.NetInvested,
                snapshot.NextCallDue?.ToString("yyyy-MM-dd")),
            rows.Select(r => new PortalActivityRowDto(
                r.Date.ToString("yyyy-MM-dd"), r.Fund, r.Type, r.Reference, r.Amount, r.Status)).ToList());
    }

    public async Task<PortalStatementsDto> GetStatementsAsync(CancellationToken ct = default)
    {
        var session = await sessionProvider.GetRequiredAsync(ct);
        var statementsAccess = await StatementsAccessAsync(session, ct);
        if (statementsAccess == "none")
            return new PortalStatementsDto(0, []);

        var documents = await db.PortalDocuments.AsNoTracking()
            .Where(d => d.InvestorId == session.InvestorId)
            .OrderByDescending(d => d.Date)
            .ToListAsync(ct);
        if (statementsAccess == "tax")
            documents = documents.Where(d => d.Type == "Tax").ToList();

        // The published library size is per investor — never another LP's count.
        var kpis = await KpiReader.ForScreenAsync(db, $"portal-statements/{session.InvestorId}", ct);
        var totalCount = statementsAccess == "full" && kpis.Count("totalCount") > 0
            ? kpis.Count("totalCount")
            : documents.Count;

        return new PortalStatementsDto(totalCount, documents.Select(d => new PortalDocumentDto(
            d.Id, d.Name, d.Fund, d.Period, d.Type, d.Date.ToString("yyyy-MM-dd"))).ToList());
    }

    public async Task<PortalTaxDto> GetTaxAsync(CancellationToken ct = default)
    {
        var session = await sessionProvider.GetRequiredAsync(ct);
        var kpis = await KpiReader.ForScreenAsync(db, "portal-tax", ct);
        var documents = await db.PortalTaxDocuments.AsNoTracking()
            .Where(d => d.InvestorId == session.InvestorId)
            .OrderByDescending(d => d.TaxYear).ThenBy(d => d.Id)
            .ToListAsync(ct);

        return new PortalTaxDto(
            new TaxBannerDto(kpis.Text("bannerHeadline"), kpis.Text("bannerDetail")),
            documents.Select(d => new PortalTaxDocumentDto(
                d.Id, d.Name, d.Fund, d.TaxYear, d.Type, d.Status, d.ExpectedDate)).ToList());
    }

    public async Task<PortalIrInfoDto> GetIrInfoAsync(CancellationToken ct = default)
    {
        var session = await sessionProvider.GetRequiredAsync(ct);
        var config = await db.PortalIrConfigs.AsNoTracking().FirstOrDefaultAsync(ct)
            ?? throw DomainException.NotFound("The IR desk is not configured.");
        var options = await db.PortalIrRegardingOptions.AsNoTracking().OrderBy(o => o.SortOrder).ToListAsync(ct);
        var requests = await db.PortalIrRequests.AsNoTracking()
            .Where(r => r.InvestorId == session.InvestorId)
            .OrderByDescending(r => r.Date).ThenByDescending(r => r.Id)
            .ToListAsync(ct);

        return new PortalIrInfoDto(
            new IrManagerDto(config.ManagerName, config.ManagerInitials, config.ManagerTitle),
            config.Email, config.Phone, config.Hours,
            options.Select(o => o.Label).ToList(),
            requests.Select(ToDto).ToList());
    }

    /// <summary>Creates a ticketed IR request from the portal contact form.</summary>
    public async Task<IrRequestDto> CreateMessageAsync(CreateIrMessageRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Subject))
            throw DomainException.Validation("A subject is required.");
        if (request.Subject.Length > 200)
            throw DomainException.Validation("The subject must be 200 characters or fewer.");
        if (string.IsNullOrWhiteSpace(request.Message))
            throw DomainException.Validation("A message is required.");
        if (request.Message.Length > 2000)
            throw DomainException.Validation("The message must be 2,000 characters or fewer.");

        var session = await sessionProvider.GetRequiredAsync(ct);
        var ticket = new PortalIrRequest
        {
            InvestorId = session.InvestorId,
            Subject = request.Subject.Trim(),
            Regarding = request.Regarding?.Trim(),
            Message = request.Message.Trim(),
            Date = clock.Today,
            Status = "Open",
        };
        db.PortalIrRequests.Add(ticket);
        await db.SaveChangesAsync(ct);

        // Derived from the identity, so refs are unique under concurrency and
        // deletions; the offset lines up with the seeded #REQ-3391 (row id 1).
        ticket.Ref = $"#REQ-{3390 + ticket.Id}";

        await audit.AppendAsync(session.ContactName, "IR request", "neutral",
            $"Portal message {ticket.Ref} · {session.InvestorId}",
            $"{request.Regarding}: {ticket.Subject}", ct);

        return ToDto(ticket);
    }

    private async Task<PortalSession> RequireCapitalAccountAccessAsync(CancellationToken ct)
    {
        var session = await sessionProvider.GetRequiredAsync(ct);
        if (session.Role == "Tax-only")
            throw DomainException.Forbidden("Tax-only contacts can access tax documents and the IR desk only.");
        return session;
    }

    private async Task<string> StatementsAccessAsync(PortalSession session, CancellationToken ct)
    {
        var contact = await db.PortalContacts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == session.ContactId, ct);
        return contact?.Statements ?? "none";
    }

    private async Task<Investor> GetInvestorAsync(string investorId, CancellationToken ct) =>
        await db.Investors.AsNoTracking().FirstOrDefaultAsync(i => i.Id == investorId, ct)
            ?? throw DomainException.NotFound($"Investor '{investorId}' was not found.");

    private async Task<PortalAccountSnapshot> GetSnapshotAsync(string investorId, CancellationToken ct) =>
        await db.PortalAccountSnapshots.AsNoTracking().FirstOrDefaultAsync(s => s.InvestorId == investorId, ct)
            ?? throw DomainException.NotFound("No capital-account statement is published for this investor yet.");

    private async Task<List<PortalFundPosition>> PositionsAsync(string investorId, CancellationToken ct) =>
        await db.PortalFundPositions.AsNoTracking()
            .Where(p => p.InvestorId == investorId)
            .OrderByDescending(p => p.Vintage)
            .ToListAsync(ct);

    private static IrRequestDto ToDto(PortalIrRequest r) =>
        new(r.Subject, r.Ref, Display.ShortDate(r.Date), r.Status);
}
