namespace Meridian.Domain.Entities;

/// <summary>External feed/connection health (5f); warnings surface in needs-attention.</summary>
public class Integration
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Direction { get; set; } = "Inbound";
    /// <summary>Display time of the last sync, e.g. "09:40".</summary>
    public string LastSync { get; set; } = "";
    public string Status { get; set; } = "Connected";
    public string? Warning { get; set; }
}

public class NotificationRule
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Trigger { get; set; } = "";
    public string Channel { get; set; } = "";
    public string Recipients { get; set; } = "";
    public bool Enabled { get; set; }
}

public class NotificationChannel
{
    public string Name { get; set; } = "";
    public string Detail { get; set; } = "";
    public bool Connected { get; set; }
}

/// <summary>
/// Published dashboard metric for KPI strips whose figures are reporting
/// outputs (not derivable sums over the seeded rows) — one row per
/// (screen, metric), numeric or text valued.
/// </summary>
public class KpiSnapshot
{
    public long Id { get; set; }
    public string ScreenKey { get; set; } = "";
    public string MetricKey { get; set; } = "";
    public decimal? NumericValue { get; set; }
    public string? TextValue { get; set; }
}
