namespace Meridian.Domain.Entities;

public class Deal
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Borrower { get; set; } = "";
    public string Sector { get; set; } = "";
    public string Country { get; set; } = "";
    public string FundId { get; set; } = "";
    public string Tranche { get; set; } = "";
    public decimal Invested { get; set; }
    public decimal Outstanding { get; set; }
    public string Spread { get; set; } = "";
    public decimal NetIrrPct { get; set; }
    public string IrrTrend { get; set; } = "flat";
    public decimal Moic { get; set; }
    public string Status { get; set; } = "Performing";
}
