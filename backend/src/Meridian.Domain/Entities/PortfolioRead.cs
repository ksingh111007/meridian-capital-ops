namespace Meridian.Domain.Entities;

public class DealCashflow
{
    public DateOnly Date { get; set; }
    public string Type { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal PrincipalBalance { get; set; }
}

public class DealLpExposure
{
    public string Investor { get; set; } = "";
    public decimal Amount { get; set; }
}

public class DealDocument
{
    public string Name { get; set; } = "";
}

/// <summary>Per-deal drill-down (terms, risk, cashflows, LP exposure) extending <see cref="Deal"/> 1:1.</summary>
public class DealDetail
{
    public string DealId { get; set; } = "";
    public decimal FairValue { get; set; }
    public decimal Facility { get; set; }
    public decimal Drawn { get; set; }
    public string Maturity { get; set; } = "";
    public string SpreadFloor { get; set; } = "";
    public decimal UpfrontFeePct { get; set; }
    public string InternalRating { get; set; } = "";
    public string RiskTrend { get; set; } = "";
    public string Covenants { get; set; } = "";
    public string NetLeverage { get; set; } = "";
    public string LastReview { get; set; } = "";
    public List<DealCashflow> Cashflows { get; set; } = [];
    public List<DealLpExposure> LpExposures { get; set; } = [];
    public List<DealDocument> Documents { get; set; } = [];
}

public class PortfolioTrendPoint
{
    /// <summary>Quarter position, oldest first.</summary>
    public int SortOrder { get; set; }
    /// <summary>Relative height 0–100.</summary>
    public int Value { get; set; }
}

/// <summary>
/// Whole-book dashboard snapshot (screen 6a). These are reporting figures the
/// platform receives from the valuation process, not sums over seeded deals —
/// stored so the API serves exactly the published numbers.
/// </summary>
public class PortfolioSnapshot
{
    public string Id { get; set; } = "current";
    public DateOnly AsOf { get; set; }
    public decimal InvestedCapital { get; set; }
    public int ActiveDeals { get; set; }
    public decimal NetIrrPct { get; set; }
    public decimal BlendedMoic { get; set; }
    public int OnWatchCount { get; set; }
    public decimal OnWatchExposure { get; set; }
    public decimal PerformingPct { get; set; }
    public decimal WatchPct { get; set; }
    public decimal NonAccrualPct { get; set; }
    public List<PortfolioTrendPoint> ValueTrend { get; set; } = [];
}

public class CashForecastBar
{
    /// <summary>Week position in the 13-week forecast.</summary>
    public int SortOrder { get; set; }
    /// <summary>Relative height 0–100.</summary>
    public int Height { get; set; }
}

public class CashForecastWeek
{
    public int SortOrder { get; set; }
    public string Label { get; set; } = "";
    public decimal Inflows { get; set; }
    public decimal Outflows { get; set; }
    public decimal Net { get; set; }
    public decimal ProjectedBalance { get; set; }
}

/// <summary>Treasury dashboard snapshot (screen 3d) — scalars plus the forecast series.</summary>
public class CashPositionSnapshot
{
    public string Id { get; set; } = "current";
    public DateOnly AsOf { get; set; }
    public string FundId { get; set; } = "";
    public decimal CashOnHand { get; set; }
    public int AccountsCount { get; set; }
    public decimal UncalledCapital { get; set; }
    public int UncalledLps { get; set; }
    public decimal FacilityHeadroom { get; set; }
    public decimal FacilityLimit { get; set; }
    public decimal Net30DayProjection { get; set; }
    public decimal CoverageRatio { get; set; }
    public List<CashForecastBar> ForecastBars { get; set; } = [];
    public List<CashForecastWeek> Weeks { get; set; } = [];
}
