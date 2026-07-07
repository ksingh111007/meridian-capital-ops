using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Meridian.Domain.Entities;

namespace Meridian.Domain.Services;

/// <summary>
/// Tamper-evident hash chain for the global audit log:
/// seal_n = SHA-256(seal_{n-1} ‖ event_n), truncated to 12 hex chars for display.
/// </summary>
public static class AuditSealer
{
    public static string ComputeSeal(string? previousSeal, DateTime at, string actor, string action, string subject, string detail)
    {
        // Kind-agnostic, culture-invariant timestamp: values round-trip through storage
        // as unspecified-kind UTC, and the seal must verify identically before and after
        // persistence and across hosts with different default cultures/calendars.
        var timestamp = at.ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);
        var payload = Encode(previousSeal ?? "", timestamp, actor, action, subject, detail);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash)[..12].ToLowerInvariant();
    }

    /// <summary>
    /// Length-prefixed field encoding: free-text fields (comments in Detail) can contain
    /// any delimiter, so plain concatenation would let distinct field splits produce the
    /// same payload and defeat tamper evidence at field boundaries.
    /// </summary>
    private static string Encode(params string[] fields) =>
        string.Concat(fields.Select(f => string.Create(
            CultureInfo.InvariantCulture, $"{f.Length}:{f}")));

    /// <summary>Verifies a chain given events in append order (oldest first).</summary>
    public static bool VerifyChain(IEnumerable<AuditEvent> eventsInAppendOrder)
    {
        string? previous = null;
        foreach (var e in eventsInAppendOrder)
        {
            if (e.Seal != ComputeSeal(previous, e.At, e.Actor, e.Action, e.Subject, e.Detail))
                return false;
            previous = e.Seal;
        }

        return true;
    }
}
