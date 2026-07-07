namespace Meridian.Domain.Entities;

/// <summary>External LP portal identity, linked to an <see cref="Investor"/>.</summary>
public class PortalContact
{
    public string Id { get; set; } = "";
    /// <summary>Display name, or the invite email until accepted.</summary>
    public string Name { get; set; } = "";
    public string Initials { get; set; } = "";
    public string InvestorId { get; set; } = "";
    public string InvestorName { get; set; } = "";
    /// <summary>Primary | Viewer | Tax-only.</summary>
    public string Role { get; set; } = "Viewer";
    public string FundsVisible { get; set; } = "";
    /// <summary>full | tax | none.</summary>
    public string Statements { get; set; } = "full";
    public string Status { get; set; } = "Active";
}

/// <summary>Portal capability toggle (admin 5g).</summary>
public class PortalCapability
{
    public int SortOrder { get; set; }
    public string Label { get; set; } = "";
    public bool Enabled { get; set; }
}

/// <summary>Document types exposed to the portal (gates downloads).</summary>
public class PortalDocumentType
{
    public int SortOrder { get; set; }
    public string Label { get; set; } = "";
    public bool Exposed { get; set; }
}

/// <summary>Capital-account position per investor × fund (portal home + investments).</summary>
public class PortalFundPosition
{
    public long Id { get; set; }
    public string InvestorId { get; set; } = "";
    public string FundId { get; set; } = "";
    public string FundName { get; set; } = "";
    public int Vintage { get; set; }
    public decimal Commitment { get; set; }
    public decimal PaidIn { get; set; }
    public decimal Distributions { get; set; }
    public decimal Nav { get; set; }
    public decimal NetIrrPct { get; set; }
    public decimal Tvpi { get; set; }
    public decimal Dpi { get; set; }
    public decimal CalledPct { get; set; }
    public decimal CalledAmount { get; set; }
}

/// <summary>Investor-level capital-account stats as of the last statement date.</summary>
public class PortalAccountSnapshot
{
    public string InvestorId { get; set; } = "";
    public DateOnly AsOf { get; set; }
    public decimal Commitment { get; set; }
    public decimal PaidIn { get; set; }
    public decimal Distributions { get; set; }
    public decimal Nav { get; set; }
    public decimal NetIrrPct { get; set; }
    public decimal Tvpi { get; set; }
    public decimal NetInvested { get; set; }
    public DateOnly? NextCallDue { get; set; }
}

/// <summary>One line of the quarterly capital-account rollforward, with per-fund amounts.</summary>
public class PortalRollforwardAmount
{
    public string FundId { get; set; } = "";
    public decimal Amount { get; set; }
}

public class PortalRollforwardLine
{
    public long Id { get; set; }
    public string InvestorId { get; set; } = "";
    public string Period { get; set; } = "";
    public int SortOrder { get; set; }
    public string Label { get; set; } = "";
    /// <summary>start | positive | negative | end.</summary>
    public string Kind { get; set; } = "";
    public decimal Total { get; set; }
    public List<PortalRollforwardAmount> Amounts { get; set; } = [];
}

/// <summary>Lifetime call/distribution ledger row for the LP.</summary>
public class PortalActivityRow
{
    public long Id { get; set; }
    public string InvestorId { get; set; } = "";
    public DateOnly Date { get; set; }
    /// <summary>Fund display label, e.g. "Fund III".</summary>
    public string Fund { get; set; } = "";
    public string Type { get; set; } = "";
    public string Reference { get; set; } = "";
    /// <summary>Negative = capital call (outflow for the LP).</summary>
    public decimal Amount { get; set; }
    public string Status { get; set; } = "";
}

public class PortalDocument
{
    public string Id { get; set; } = "";
    public string InvestorId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Fund { get; set; } = "";
    public string Period { get; set; } = "";
    public string Type { get; set; } = "";
    public DateOnly Date { get; set; }
}

public class PortalTaxDocument
{
    public string Id { get; set; } = "";
    public string InvestorId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Fund { get; set; } = "";
    public int TaxYear { get; set; }
    public string Type { get; set; } = "";
    /// <summary>Available | Pending — pending docs are never downloadable.</summary>
    public string Status { get; set; } = "Available";
    public string? ExpectedDate { get; set; }
}

/// <summary>IR desk configuration shown on the portal contact screen (single row).</summary>
public class PortalIrConfig
{
    public string Id { get; set; } = "current";
    public string ManagerName { get; set; } = "";
    public string ManagerInitials { get; set; } = "";
    public string ManagerTitle { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Hours { get; set; } = "";
}

public class PortalIrRegardingOption
{
    public int SortOrder { get; set; }
    public string Label { get; set; } = "";
}

/// <summary>Ticketed IR request created from the portal contact form.</summary>
public class PortalIrRequest
{
    public long Id { get; set; }
    public string InvestorId { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Ref { get; set; } = "";
    public DateOnly Date { get; set; }
    public string Status { get; set; } = "Open";
}
