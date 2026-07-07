using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Meridian.Api.IntegrationTests.Support;
using Xunit;

namespace Meridian.Api.IntegrationTests;

/// <summary>Wire retry and recon assignment — RBAC-gated, audited mutations.</summary>
public class FundOpsMutationTests(MeridianApiFactory factory) : IClassFixture<MeridianApiFactory>
{
    [Fact]
    public async Task RetryingTheExceptionWire_RequeuesItAndAudits()
    {
        var client = factory.CreateClientFor(Users.OpsManager);

        var response = await client.PostAsync("/api/wires/wire-8847/retry", null);
        var wire = await response.ReadJsonAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Queued", (string?)wire["status"]);
        Assert.Null((string?)wire["exceptionReason"]);

        var wires = await client.GetJsonAsync("/api/wires");
        Assert.DoesNotContain(wires["wires"]!.AsArray(), w => (string?)w!["status"] == "Exception");

        var audit = await factory.CreateClientFor(Users.Admin).GetJsonAsync("/api/admin/audit");
        Assert.Contains(audit["events"]!.AsArray(),
            e => (string?)e!["action"] == "Wire retried" && ((string?)e["object"])!.Contains("W-8847"));
        Assert.True((bool?)audit["kpis"]!["chainValid"]);
    }

    [Fact]
    public async Task RetryingANonExceptionWire_Conflicts()
    {
        var response = await factory.CreateClientFor(Users.OpsManager).PostAsync("/api/wires/wire-8842/retry", null);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task RetryingAWire_RequiresWiresEdit()
    {
        // CIO has Wires:view — enough to look, not to retry.
        var response = await factory.CreateClientFor(Users.Cio).PostAsync("/api/wires/wire-8847/retry", null);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AssigningABreak_SetsTheOwnerAndAudits()
    {
        var client = factory.CreateClientFor(Users.OpsAnalyst);

        var response = await client.PostAsJsonAsync("/api/reconciliation/rec-3/assign", new { assignee = "D. Whitfield" });
        var item = await response.ReadJsonAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("D. Whitfield", (string?)item["assignee"]);

        var audit = await factory.CreateClientFor(Users.Admin).GetJsonAsync("/api/admin/audit");
        Assert.Contains(audit["events"]!.AsArray(), e => (string?)e!["action"] == "Recon assigned");
    }

    [Fact]
    public async Task AssigningAMatchedItem_Conflicts()
    {
        var response = await factory.CreateClientFor(Users.OpsAnalyst)
            .PostAsJsonAsync("/api/reconciliation/rec-1/assign", new { assignee = "Anyone" });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
