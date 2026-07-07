using Meridian.Application.Abstractions;
using Meridian.Domain;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Application.FundOps;

public sealed record CashWeekDto(string Label, decimal Inflows, decimal Outflows, decimal Net, decimal ProjectedBalance);

public sealed record CashAccountDto(string Custodian, string Account, string Currency, string Type, decimal Balance);

public sealed record CashPositionDto(
    string AsOf, string FundId, decimal CashOnHand, int AccountsCount, decimal UncalledCapital, int UncalledLps,
    decimal FacilityHeadroom, decimal FacilityLimit, decimal Net30DayProjection, IReadOnlyList<int> ForecastBars,
    decimal CoverageRatio, IReadOnlyList<CashWeekDto> Weeks, IReadOnlyList<CashAccountDto> Accounts);

public class CashPositionService(IAppDbContext db)
{
    public async Task<CashPositionDto> GetAsync(CancellationToken ct = default)
    {
        var snapshot = await db.CashPositionSnapshots.AsNoTracking().FirstOrDefaultAsync(ct)
            ?? throw DomainException.NotFound("No cash-position snapshot is published.");
        var accounts = await db.CashAccounts.AsNoTracking().OrderBy(a => a.Id).ToListAsync(ct);

        return new CashPositionDto(
            snapshot.AsOf.ToString("yyyy-MM-dd"), snapshot.FundId, snapshot.CashOnHand, snapshot.AccountsCount,
            snapshot.UncalledCapital, snapshot.UncalledLps, snapshot.FacilityHeadroom, snapshot.FacilityLimit,
            snapshot.Net30DayProjection,
            snapshot.ForecastBars.OrderBy(b => b.SortOrder).Select(b => b.Height).ToList(),
            snapshot.CoverageRatio,
            snapshot.Weeks.OrderBy(w => w.SortOrder)
                .Select(w => new CashWeekDto(w.Label, w.Inflows, w.Outflows, w.Net, w.ProjectedBalance)).ToList(),
            accounts.Select(a => new CashAccountDto(a.Custodian, a.Account, a.Currency, a.Type, a.Balance)).ToList());
    }
}
