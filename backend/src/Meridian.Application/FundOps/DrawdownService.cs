using Meridian.Application.Abstractions;
using Meridian.Application.Common;
using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Application.FundOps;

public sealed record DrawdownKpisDto(
    decimal FacilityLimit, int Facilities, decimal Drawn, decimal UtilisationPct, decimal Available, string WeightedRate);

public sealed record DrawdownDto(
    string Id, string Facility, string Lender, string Purpose, string? DealId, string? LinkedCallId,
    decimal Amount, string Rate, string DrawDate, string? RepayBy, string Status);

public sealed record DrawdownsDto(DrawdownKpisDto Kpis, IReadOnlyList<DrawdownDto> Drawdowns);

public class DrawdownService(IAppDbContext db)
{
    public async Task<DrawdownsDto> GetAsync(CancellationToken ct = default)
    {
        var kpis = await KpiReader.ForScreenAsync(db, "drawdowns", ct);
        var draws = await db.Drawdowns.AsNoTracking().OrderBy(d => d.Id).ToListAsync(ct);

        return new DrawdownsDto(
            new DrawdownKpisDto(
                kpis.Number("facilityLimit"), kpis.Count("facilities"), kpis.Number("drawn"),
                kpis.Number("utilisationPct"), kpis.Number("available"), kpis.Text("weightedRate")),
            draws.Select(d => new DrawdownDto(
                d.Id, d.Facility, d.Lender, d.Purpose, d.DealId, d.LinkedCallId, d.Amount, d.Rate,
                d.DrawDate.ToString("yyyy-MM-dd"), d.RepayBy?.ToString("yyyy-MM-dd"), d.Status)).ToList());
    }
}
