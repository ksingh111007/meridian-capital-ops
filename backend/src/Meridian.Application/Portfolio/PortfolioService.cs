using Meridian.Application.Abstractions;
using Meridian.Domain;
using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Application.Portfolio;

public sealed record ExposureMixDto(decimal PerformingPct, decimal WatchPct, decimal NonAccrualPct);

public sealed record PortfolioSummaryDto(
    string AsOf, decimal InvestedCapital, int ActiveDeals, decimal NetIrrPct, decimal BlendedMoic,
    int OnWatchCount, decimal OnWatchExposure, IReadOnlyList<int> ValueTrend, ExposureMixDto ExposureMix);

public sealed record DealDto(
    string Id, string Name, string Borrower, string Sector, string Country, string FundId, string Tranche,
    decimal Invested, decimal Outstanding, string Spread, decimal NetIrrPct, string IrrTrend, decimal Moic,
    string Status);

public sealed record DealCashflowDto(string Date, string Type, decimal Amount, decimal PrincipalBalance);

public sealed record DealRiskDto(string InternalRating, string Trend, string Covenants, string NetLeverage, string LastReview);

public sealed record DealLpExposureDto(string Investor, decimal Amount);

/// <summary>The full drill-down — <see cref="DealDto"/> plus terms/cashflows/risk/exposure/documents.</summary>
public sealed record DealDetailDto(
    string Id, string Name, string Borrower, string Sector, string Country, string FundId, string Tranche,
    decimal Invested, decimal Outstanding, string Spread, decimal NetIrrPct, string IrrTrend, decimal Moic,
    string Status,
    decimal FairValue, decimal Facility, decimal Drawn, string Maturity, string SpreadFloor, decimal UpfrontFeePct,
    IReadOnlyList<DealCashflowDto> Cashflows, DealRiskDto Risk, IReadOnlyList<DealLpExposureDto> LpExposure,
    IReadOnlyList<string> Documents);

public class PortfolioService(IAppDbContext db)
{
    public async Task<PortfolioSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var snapshot = await db.PortfolioSnapshots.AsNoTracking().FirstOrDefaultAsync(ct)
            ?? throw DomainException.NotFound("No portfolio snapshot is published.");

        return new PortfolioSummaryDto(
            snapshot.AsOf.ToString("yyyy-MM-dd"), snapshot.InvestedCapital, snapshot.ActiveDeals,
            snapshot.NetIrrPct, snapshot.BlendedMoic, snapshot.OnWatchCount, snapshot.OnWatchExposure,
            snapshot.ValueTrend.OrderBy(p => p.SortOrder).Select(p => p.Value).ToList(),
            new ExposureMixDto(snapshot.PerformingPct, snapshot.WatchPct, snapshot.NonAccrualPct));
    }

    public async Task<IReadOnlyList<DealDto>> ListDealsAsync(CancellationToken ct = default)
    {
        var deals = await db.Deals.AsNoTracking().OrderBy(d => d.Name).ToListAsync(ct);
        return deals.Select(ToDto).ToList();
    }

    public async Task<DealDetailDto> GetDealAsync(string id, CancellationToken ct = default)
    {
        var deal = await db.Deals.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct)
            ?? throw DomainException.NotFound($"Deal '{id}' was not found.");
        var detail = await db.DealDetails.AsNoTracking().FirstOrDefaultAsync(d => d.DealId == id, ct)
            ?? throw DomainException.NotFound($"Deal '{id}' has no detail record.");

        return new DealDetailDto(
            deal.Id, deal.Name, deal.Borrower, deal.Sector, deal.Country, deal.FundId, deal.Tranche,
            deal.Invested, deal.Outstanding, deal.Spread, deal.NetIrrPct, deal.IrrTrend, deal.Moic, deal.Status,
            detail.FairValue, detail.Facility, detail.Drawn, detail.Maturity, detail.SpreadFloor, detail.UpfrontFeePct,
            detail.Cashflows.OrderByDescending(c => c.Date).Select(c =>
                new DealCashflowDto(c.Date.ToString("yyyy-MM-dd"), c.Type, c.Amount, c.PrincipalBalance)).ToList(),
            new DealRiskDto(detail.InternalRating, detail.RiskTrend, detail.Covenants, detail.NetLeverage, detail.LastReview),
            detail.LpExposures.OrderByDescending(x => x.Amount).Select(x => new DealLpExposureDto(x.Investor, x.Amount)).ToList(),
            detail.Documents.Select(d => d.Name).ToList());
    }

    private static DealDto ToDto(Deal d) => new(
        d.Id, d.Name, d.Borrower, d.Sector, d.Country, d.FundId, d.Tranche,
        d.Invested, d.Outstanding, d.Spread, d.NetIrrPct, d.IrrTrend, d.Moic, d.Status);
}
