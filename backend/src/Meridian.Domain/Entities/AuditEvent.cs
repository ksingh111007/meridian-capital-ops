namespace Meridian.Domain.Entities;

/// <summary>
/// Global append-only audit log. <see cref="Seal"/> is hash-chained:
/// seal_n = H(seal_{n-1} ‖ event_n) — see Services/AuditSealer.
/// </summary>
public class AuditEvent
{
    public long Id { get; set; }
    public DateTime At { get; set; }
    public string Actor { get; set; } = "";
    public string Action { get; set; } = "";
    public string Tone { get; set; } = "neutral";
    /// <summary>Serialized to the API as "object".</summary>
    public string Subject { get; set; } = "";
    public string Detail { get; set; } = "";
    public string Seal { get; set; } = "";

    /// <summary>
    /// The seal this event chains from (null for the genesis event). A unique
    /// index enforces one successor per seal, so concurrent writers or a retried
    /// commit cannot silently fork the chain — the second insert fails instead.
    /// </summary>
    public string? PreviousSeal { get; set; }
}

/// <summary>Outbox row written by the notification port's default implementation.</summary>
public class Notification
{
    public long Id { get; set; }
    public DateTime At { get; set; }
    public string RecipientRole { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
}
