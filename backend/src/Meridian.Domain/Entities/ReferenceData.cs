namespace Meridian.Domain.Entities;

public class LegalEntity
{
    public long Id { get; set; }
    public string FundId { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>GP | Master | Feeder | Cayman | Blocker.</summary>
    public string Kind { get; set; } = "";
}

/// <summary>Share-class terms — the waterfall computation inputs.</summary>
public class ShareClass
{
    public long Id { get; set; }
    public string FundId { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal MgmtFeePct { get; set; }
    public decimal CarryPct { get; set; }
    public decimal PrefPct { get; set; }
}

/// <summary>Banking/KYC profile shown on the investor registry drawer (5d).</summary>
public class InvestorProfile
{
    public string InvestorId { get; set; } = "";
    public string Bank { get; set; } = "";
    public string AbaMasked { get; set; } = "";
    public string AccountMasked { get; set; } = "";
    public string BankingVerified { get; set; } = "";
    public string KycDocs { get; set; } = "";
    public string KycReviewDue { get; set; } = "";
}

public class Borrower
{
    public string Name { get; set; } = "";
    public string Sector { get; set; } = "";
    public string Country { get; set; } = "";
    public string DealName { get; set; } = "";
    public string InternalRating { get; set; } = "";
}

public class CurrencyRate
{
    public string Code { get; set; } = "";
    public decimal Rate { get; set; }
    public string Note { get; set; } = "";
}

public class SettlementCalendar
{
    public string Name { get; set; } = "";
    public string NextHoliday { get; set; } = "";
}
