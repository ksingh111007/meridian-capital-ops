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
        // Kind-agnostic timestamp: values round-trip through storage as unspecified-kind
        // UTC, and the seal must verify identically before and after persistence.
        var payload = $"{previousSeal}|{at:yyyy-MM-ddTHH:mm:ss.fffffff}|{actor}|{action}|{subject}|{detail}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash)[..12].ToLowerInvariant();
    }

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
