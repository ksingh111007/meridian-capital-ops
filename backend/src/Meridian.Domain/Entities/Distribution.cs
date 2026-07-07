namespace Meridian.Domain.Entities;

public class WaterfallTier
{
    /// <summary>Display label, e.g. "1 · Return of Capital" — ordered lexically.</summary>
    public string Tier { get; set; } = "";
    public string Basis { get; set; } = "";
    public string Rate { get; set; } = "";
    public decimal Distributed { get; set; }
    public decimal? LpShare { get; set; }
    public decimal? GpShare { get; set; }
    public decimal PoolLeft { get; set; }
}

public class InvestorPayout
{
    public string InvestorId { get; set; } = "";
    public string InvestorName { get; set; } = "";
    public decimal Commitment { get; set; }
    public decimal Amount { get; set; }
    public decimal PctOfLpTotal { get; set; }
    public PayoutStatus Status { get; set; }
    /// <summary>Set exactly when the LP has no wire instructions on file.</summary>
    public string? BlockedReason { get; set; }
    public string? WireRef { get; set; }
}

public class Distribution
{
    public string Id { get; set; } = "";
    public string Ref { get; set; } = "";
    public string FundId { get; set; } = "";
    public decimal Distributable { get; set; }
    public decimal LpTotal { get; set; }
    public decimal GpTotal { get; set; }
    public DateOnly PaymentDate { get; set; }
    public DistributionStatus Status { get; set; }
    public WaterfallType WaterfallType { get; set; }
    public string SourceNote { get; set; } = "";
    /// <summary>Recallable distributions restore unfunded commitment (LPA-dependent) — modeled per BACKEND_TODO.</summary>
    public bool Recallable { get; set; }
    public List<WaterfallTier> Tiers { get; set; } = [];
    public List<InvestorPayout> Payouts { get; set; } = [];
}
