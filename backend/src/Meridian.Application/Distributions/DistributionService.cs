using Meridian.Application.Abstractions;
using Meridian.Domain;
using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Application.Distributions;

public sealed record WaterfallTierDto(
    string Tier, string Basis, string Rate, decimal Distributed, decimal? LpShare, decimal? GpShare, decimal PoolLeft);

public sealed record InvestorPayoutDto(
    string InvestorId, string Investor, decimal Commitment, decimal Amount, decimal PctOfLpTotal,
    string Status, string? BlockedReason, string? WireRef);

public sealed record DistributionDto(
    string Id, string Ref, string FundId, decimal Distributable, decimal LpTotal, decimal GpTotal,
    DateOnly PaymentDate, string Status, string WaterfallType, string SourceNote, bool Recallable,
    IReadOnlyList<WaterfallTierDto> Tiers, IReadOnlyList<InvestorPayoutDto> Payouts);

public class DistributionService(IAppDbContext db)
{
    public async Task<IReadOnlyList<DistributionDto>> ListAsync(CancellationToken ct = default)
    {
        var distributions = await db.Distributions.AsNoTracking().ToListAsync(ct);
        return distributions.OrderByDescending(d => d.Id).Select(ToDto).ToList();
    }

    public async Task<DistributionDto> GetAsync(string id, CancellationToken ct = default)
    {
        var distribution = await db.Distributions.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct)
            ?? throw DomainException.NotFound($"Distribution '{id}' was not found.");
        return ToDto(distribution);
    }

    private static DistributionDto ToDto(Distribution d) => new(
        d.Id, d.Ref, d.FundId, d.Distributable, d.LpTotal, d.GpTotal, d.PaymentDate,
        d.Status.ToString(), d.WaterfallType.ToString(), d.SourceNote, d.Recallable,
        d.Tiers.OrderBy(t => t.Tier).Select(t => new WaterfallTierDto(
            t.Tier, t.Basis, t.Rate, t.Distributed, t.LpShare, t.GpShare, t.PoolLeft)).ToList(),
        d.Payouts.OrderByDescending(p => p.Amount).ThenBy(p => p.InvestorId).Select(p => new InvestorPayoutDto(
            p.InvestorId, p.InvestorName, p.Commitment, p.Amount, p.PctOfLpTotal,
            p.Status.ToString(), p.BlockedReason, p.WireRef)).ToList());
}
