namespace Meridian.Domain.Entities;

/// <summary>Subscription-line / NAV-facility draw bridging a deal until its call lands.</summary>
public class Drawdown
{
    public string Id { get; set; } = "";
    public string Facility { get; set; } = "";
    public string Lender { get; set; } = "";
    public string Purpose { get; set; } = "";
    public string? DealId { get; set; }
    public string? LinkedCallId { get; set; }
    public decimal Amount { get; set; }
    public string Rate { get; set; } = "";
    public DateOnly DrawDate { get; set; }
    public DateOnly? RepayBy { get; set; }
    public string Status { get; set; } = "Outstanding";
}

public class Wire
{
    public string Id { get; set; } = "";
    public string Ref { get; set; } = "";
    public string Direction { get; set; } = "In";
    public string Counterparty { get; set; } = "";
    public string Type { get; set; } = "";
    /// <summary>"#C-…" / "#D-…" / facility name.</summary>
    public string LinkedRef { get; set; } = "";
    public decimal Amount { get; set; }
    /// <summary>Display time "09:02" — the contract sends time and date separately.</summary>
    public string Time { get; set; } = "";
    public DateOnly Date { get; set; }
    public string Rail { get; set; } = "Fedwire";
    public string Status { get; set; } = "Queued";
    public string? ExceptionReason { get; set; }
}

public class ReconItem
{
    public string Id { get; set; } = "";
    public DateOnly Date { get; set; }
    public string Description { get; set; } = "";
    public string Source { get; set; } = "";
    public decimal? Book { get; set; }
    public decimal? Custodian { get; set; }
    public decimal Diff { get; set; }
    public string Status { get; set; } = "Matched";
    public string? Assignee { get; set; }
}

public class CashAccount
{
    public long Id { get; set; }
    public string Custodian { get; set; } = "";
    public string Account { get; set; } = "";
    public string Currency { get; set; } = "USD";
    public string Type { get; set; } = "Operating";
    public decimal Balance { get; set; }
}
