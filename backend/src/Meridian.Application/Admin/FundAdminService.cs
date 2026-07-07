using Meridian.Application.Abstractions;
using Meridian.Application.Common;
using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Application.Admin;

public sealed record FundKpisDto(
    int Funds, int Investing, decimal Committed, decimal CalledPct, decimal CalledAmount, int LegalEntities);

public sealed record FundDto(
    string Id, string Name, string ShortName, int Vintage, decimal Committed, decimal CalledPct,
    string Strategy, string WaterfallType, string BaseCurrency, string Status);

public sealed record LegalEntityDto(string FundId, string Name, string Kind);

public sealed record ShareClassDto(string FundId, string Name, decimal MgmtFeePct, decimal CarryPct, decimal PrefPct);

public sealed record FundsDto(
    FundKpisDto Kpis, IReadOnlyList<FundDto> Funds, IReadOnlyList<LegalEntityDto> Entities,
    IReadOnlyList<ShareClassDto> ShareClasses);

public sealed record InvestorKpisDto(
    int Investors, decimal Commitments, decimal KycVerifiedPct, int KycInReview, decimal WireOnFilePct, int WireMissing);

public sealed record InvestorCommitmentDto(string FundId, decimal Amount, decimal Called);

public sealed record InvestorProfileDto(
    string Bank, string AbaMasked, string AccountMasked, string BankingVerified, string KycDocs, string KycReviewDue);

public sealed record InvestorDto(
    string Id, string Name, string Type, IReadOnlyList<InvestorCommitmentDto> Commitments,
    string KycStatus, bool WireInstructionsOnFile, InvestorProfileDto? Profile);

public sealed record InvestorsDto(InvestorKpisDto Kpis, IReadOnlyList<InvestorDto> Investors);

public sealed record BorrowerDto(string Name, string Sector, string Country, string Deal, string InternalRating);

public sealed record CurrencyDto(string Code, decimal Rate, string Note);

public sealed record CalendarDto(string Name, string NextHoliday);

public sealed record ReferenceDataDto(
    IReadOnlyList<BorrowerDto> Borrowers, IReadOnlyList<CurrencyDto> Currencies, string CurrenciesUpdated,
    IReadOnlyList<CalendarDto> Calendars);

public class FundAdminService(IAppDbContext db)
{
    public async Task<FundsDto> GetFundsAsync(CancellationToken ct = default)
    {
        var kpis = await KpiReader.ForScreenAsync(db, "funds", ct);
        var funds = await db.Funds.AsNoTracking().OrderByDescending(f => f.Vintage).ToListAsync(ct);
        var entities = await db.LegalEntities.AsNoTracking().OrderBy(e => e.Id).ToListAsync(ct);
        var shareClasses = await db.ShareClasses.AsNoTracking().OrderBy(s => s.Id).ToListAsync(ct);

        return new FundsDto(
            new FundKpisDto(
                kpis.Count("funds"), kpis.Count("investing"), kpis.Number("committed"),
                kpis.Number("calledPct"), kpis.Number("calledAmount"), kpis.Count("legalEntities")),
            funds.Select(f => new FundDto(
                f.Id, f.Name, f.ShortName, f.Vintage, f.Committed, f.CalledPct, f.Strategy,
                f.WaterfallType.ToString(), f.BaseCurrency, f.Status.ToString())).ToList(),
            entities.Select(e => new LegalEntityDto(e.FundId, e.Name, e.Kind)).ToList(),
            shareClasses.Select(s => new ShareClassDto(s.FundId, s.Name, s.MgmtFeePct, s.CarryPct, s.PrefPct)).ToList());
    }

    public async Task<InvestorsDto> GetInvestorsAsync(CancellationToken ct = default)
    {
        var kpis = await KpiReader.ForScreenAsync(db, "investors", ct);
        var investors = await db.Investors.AsNoTracking().OrderBy(i => i.Id).ToListAsync(ct);
        var profiles = await db.InvestorProfiles.AsNoTracking().ToDictionaryAsync(p => p.InvestorId, ct);

        return new InvestorsDto(
            new InvestorKpisDto(
                kpis.Count("investors"), kpis.Number("commitments"), kpis.Number("kycVerifiedPct"),
                kpis.Count("kycInReview"), kpis.Number("wireOnFilePct"), kpis.Count("wireMissing")),
            investors.Select(i => new InvestorDto(
                i.Id, i.Name, i.Type,
                i.Commitments.Select(c => new InvestorCommitmentDto(c.FundId, c.Amount, c.Called)).ToList(),
                i.KycStatus, i.WireInstructionsOnFile,
                profiles.TryGetValue(i.Id, out var profile) ? ToDto(profile) : null)).ToList());
    }

    public async Task<ReferenceDataDto> GetReferenceDataAsync(CancellationToken ct = default)
    {
        var kpis = await KpiReader.ForScreenAsync(db, "reference-data", ct);
        var borrowers = await db.Borrowers.AsNoTracking().OrderBy(b => b.DealName).ToListAsync(ct);
        var currencies = await db.CurrencyRates.AsNoTracking().OrderBy(c => c.Code == "USD" ? "" : c.Code).ToListAsync(ct);
        var calendars = await db.SettlementCalendars.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

        return new ReferenceDataDto(
            borrowers.Select(b => new BorrowerDto(b.Name, b.Sector, b.Country, b.DealName, b.InternalRating)).ToList(),
            currencies.Select(c => new CurrencyDto(c.Code, c.Rate, c.Note)).ToList(),
            kpis.Text("currenciesUpdated"),
            calendars.Select(c => new CalendarDto(c.Name, c.NextHoliday)).ToList());
    }

    private static InvestorProfileDto ToDto(InvestorProfile p) =>
        new(p.Bank, p.AbaMasked, p.AccountMasked, p.BankingVerified, p.KycDocs, p.KycReviewDue);
}
