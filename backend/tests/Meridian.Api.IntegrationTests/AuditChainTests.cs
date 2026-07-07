using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Meridian.Api.IntegrationTests.Support;
using Meridian.Domain.Entities;
using Meridian.Domain.Services;
using Xunit;

namespace Meridian.Api.IntegrationTests;

public class AuditChainTests(MeridianApiFactory factory) : IClassFixture<MeridianApiFactory>
{
    [Fact]
    public async Task AuditLog_AfterMutations_StaysChainedAndVerifiable()
    {
        // Perform an audited mutation first so the chain extends beyond the seed.
        var approve = await factory.CreateClientFor(Users.Counsel)
            .PostAsJsonAsync("/api/capital-calls/call-2041/approve", new { comment = "Clear to proceed" });
        Assert.True(approve.IsSuccessStatusCode);

        var log = await factory.CreateClientFor(Users.Compliance).GetJsonAsync("/api/admin/audit");

        Assert.True((bool?)log["kpis"]!["chainValid"]);
        var events = log["events"]!.AsArray();
        Assert.True(events.Count >= 7); // 6 seeded + the approval (+ notifications don't audit)
        Assert.Contains(events, e => (string?)e!["action"] == "Approved"
            && ((string?)e!["object"])!.Contains("#C-2041 · Legal"));

        // Independently recompute the hash chain from the response payload.
        var replayed = events
            .Select(e => new AuditEvent
            {
                At = DateTime.Parse((string)e!["atUtc"]!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                Actor = (string)e["actor"]!,
                Action = (string)e["action"]!,
                Subject = (string)e["object"]!,
                Detail = (string?)e["detail"] ?? "",
                Seal = (string)e["seal"]!,
            })
            .OrderBy(e => e.At)
            .ToList();
        Assert.True(AuditSealer.VerifyChain(replayed));
    }

    [Fact]
    public async Task AuditKpis_CountTodayAndExceptions()
    {
        var log = await factory.CreateClientFor(Users.Compliance).GetJsonAsync("/api/admin/audit");
        var kpis = log["kpis"]!;
        Assert.True((int?)kpis["exceptions"] >= 1); // seeded W-8847 SWIFT exception
        Assert.True((int?)kpis["approvals"] >= 1); // the mock story seeds one CIO approval
    }
}
