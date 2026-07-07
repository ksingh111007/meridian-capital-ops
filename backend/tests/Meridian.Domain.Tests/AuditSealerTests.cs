using Meridian.Domain.Entities;
using Meridian.Domain.Services;
using Xunit;

namespace Meridian.Domain.Tests;

public class AuditSealerTests
{
    private static List<AuditEvent> Chain()
    {
        var events = new List<AuditEvent>();
        string? previous = null;
        foreach (var i in Enumerable.Range(1, 4))
        {
            var at = new DateTime(2026, 7, i, 9, 0, 0, DateTimeKind.Utc);
            var seal = AuditSealer.ComputeSeal(previous, at, $"actor-{i}", "Approved", $"Call #C-{i}", "detail");
            events.Add(new AuditEvent { At = at, Actor = $"actor-{i}", Action = "Approved", Subject = $"Call #C-{i}", Detail = "detail", Seal = seal });
            previous = seal;
        }

        return events;
    }

    [Fact]
    public void VerifyChain_AcceptsIntactChain() => Assert.True(AuditSealer.VerifyChain(Chain()));

    [Fact]
    public void VerifyChain_DetectsTamperedDetail()
    {
        var events = Chain();
        events[1].Detail = "rewritten history";
        Assert.False(AuditSealer.VerifyChain(events));
    }

    [Fact]
    public void VerifyChain_DetectsRemovedEvent()
    {
        var events = Chain();
        events.RemoveAt(1);
        Assert.False(AuditSealer.VerifyChain(events));
    }

    [Fact]
    public void Seal_DependsOnPreviousSeal()
    {
        var at = new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc);
        var a = AuditSealer.ComputeSeal(null, at, "a", "b", "c", "d");
        var b = AuditSealer.ComputeSeal("something", at, "a", "b", "c", "d");
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Seal_DetectsFieldBoundaryShifts_EvenWithDelimitersInFreeText()
    {
        // Free-text comments land in Detail; shifting text across field boundaries
        // must change the seal (length-prefixed encoding, not plain concatenation).
        var at = new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc);
        var a = AuditSealer.ComputeSeal(null, at, "actor|x", "action", "subject", "detail");
        var b = AuditSealer.ComputeSeal(null, at, "actor", "|xaction", "subject", "detail");
        Assert.NotEqual(a, b);
    }
}
