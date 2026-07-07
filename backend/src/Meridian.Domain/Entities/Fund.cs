namespace Meridian.Domain.Entities;

/// <summary>All money amounts across the domain are USD millions.</summary>
public class Fund
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string ShortName { get; set; } = "";
    public int Vintage { get; set; }
    public decimal Committed { get; set; }
    public decimal CalledPct { get; set; }
    public string Strategy { get; set; } = "";
    public WaterfallType WaterfallType { get; set; }
    public string BaseCurrency { get; set; } = "USD";
    public FundStatus Status { get; set; }
}
