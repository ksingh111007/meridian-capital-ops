using System.Net;
using System.Text.Json.Nodes;
using Meridian.Api.IntegrationTests.Support;
using Xunit;

namespace Meridian.Api.IntegrationTests;

/// <summary>
/// The read models added for the frontend swap (docs/API.md), served from the
/// mock-story seed: portfolio, deal drill-down, fund ops, admin, and reference.
/// </summary>
public class ReadModelTests(MeridianApiFactory factory) : IClassFixture<MeridianApiFactory>
{
    [Fact]
    public async Task PortfolioSummary_ServesThePublishedSnapshot()
    {
        var summary = await factory.CreateClientFor(Users.OpsAnalyst).GetJsonAsync("/api/portfolio/summary");

        Assert.Equal("2026-07-05", (string?)summary["asOf"]);
        Assert.Equal(520m, (decimal?)summary["investedCapital"]);
        Assert.Equal(8, summary["valueTrend"]!.AsArray().Count);
        Assert.Equal(84m, (decimal?)summary["exposureMix"]!["performingPct"]);
    }

    [Fact]
    public async Task DealDetail_MergesTheDealRowWithItsDrilldown()
    {
        var deal = await factory.CreateClientFor(Users.OpsAnalyst).GetJsonAsync("/api/deals/deal-atlas");

        Assert.Equal("Project Atlas", (string?)deal["name"]);
        Assert.Equal(86.1m, (decimal?)deal["fairValue"]);
        Assert.Equal("B+", (string?)deal["risk"]!["internalRating"]);
        Assert.True(deal["cashflows"]!.AsArray().Count > 0);
        Assert.True(deal["lpExposure"]!.AsArray().Count > 0);
    }

    [Fact]
    public async Task Drawdowns_LinkBackToTheirCalls()
    {
        var drawdowns = await factory.CreateClientFor(Users.OpsAnalyst).GetJsonAsync("/api/drawdowns");

        Assert.Equal(300m, (decimal?)drawdowns["kpis"]!["facilityLimit"]);
        var atlasBridge = drawdowns["drawdowns"]!.AsArray().Single(d => (string?)d!["id"] == "draw-1")!;
        Assert.Equal("call-2041", (string?)atlasBridge["linkedCallId"]);
    }

    [Fact]
    public async Task Wires_SurfaceTheSwiftException()
    {
        var wires = await factory.CreateClientFor(Users.OpsManager).GetJsonAsync("/api/wires");

        var exception = wires["wires"]!.AsArray().Single(w => (string?)w!["status"] == "Exception")!;
        Assert.Equal("W-8847", (string?)exception["ref"]);
        Assert.Contains("SWIFT", (string?)exception["exceptionReason"]);
        Assert.Equal(1, (int?)wires["kpis"]!["exceptions"]);
    }

    [Fact]
    public async Task CashPosition_CarriesForecastAndAccounts()
    {
        var cash = await factory.CreateClientFor(Users.OpsManager).GetJsonAsync("/api/cash/position");

        Assert.Equal(13, cash["forecastBars"]!.AsArray().Count);
        Assert.Equal(4, cash["weeks"]!.AsArray().Count);
        Assert.Equal(3, cash["accounts"]!.AsArray().Count);
    }

    [Fact]
    public async Task Reconciliation_ShowsTheDistributionBreak()
    {
        var recon = await factory.CreateClientFor(Users.OpsAnalyst).GetJsonAsync("/api/reconciliation");

        var breakItem = recon["items"]!.AsArray().Single(i => (string?)i!["status"] == "Break")!;
        Assert.Equal(0.42m, (decimal?)breakItem["diff"]);
        Assert.Contains("#D-119", (string?)breakItem["description"]);
    }

    [Fact]
    public async Task AdminReads_ServeDirectoryFundsInvestorsAndConfig()
    {
        var client = factory.CreateClientFor(Users.Admin);

        var users = await client.GetJsonAsync("/api/admin/users");
        Assert.Contains(users["roles"]!.AsArray(),
            r => (string?)r!["name"] == "Counsel" && (string?)r["capabilities"]!["Ref Data"] == "view");

        var funds = await client.GetJsonAsync("/api/admin/funds");
        Assert.Equal(5, funds["entities"]!.AsArray().Count);
        Assert.Equal(2, funds["shareClasses"]!.AsArray().Count);

        var investors = await client.GetJsonAsync("/api/admin/investors");
        var redwood = investors["investors"]!.AsArray().Single(i => (string?)i!["id"] == "inv-redwood")!;
        Assert.Equal("Northern Trust", (string?)redwood["profile"]!["bank"]);
        var oakmont = investors["investors"]!.AsArray().Single(i => (string?)i!["id"] == "inv-oakmont")!;
        Assert.False((bool?)oakmont["wireInstructionsOnFile"]);

        var reference = await client.GetJsonAsync("/api/admin/reference");
        Assert.Equal(7, reference["borrowers"]!.AsArray().Count);
        Assert.Equal("USD", (string?)reference["currencies"]!.AsArray()[0]!["code"]);

        var integrations = await client.GetJsonAsync("/api/admin/integrations");
        Assert.Contains(integrations["integrations"]!.AsArray(),
            i => (string?)i!["name"] == "SWIFT Gateway" && (string?)i["status"] == "Warning");

        var notificationRules = await client.GetJsonAsync("/api/admin/notification-rules");
        Assert.Equal(7, notificationRules["rules"]!.AsArray().Count);

        var access = await client.GetJsonAsync("/api/admin/investor-access");
        Assert.Equal("44 / 48", (string?)access["kpis"]!["investorsWithAccess"]);
        Assert.Equal(6, access["contacts"]!.AsArray().Count);
    }

    [Fact]
    public async Task NeedsAttention_IncludesExceptionsBreaksAndWarnings()
    {
        var items = (await factory.CreateClientFor(Users.OpsAnalyst).GetJsonAsync("/api/needs-attention")).AsArray();

        Assert.Contains(items, i => (string?)i!["kind"] == "exception" && ((string?)i!["title"])!.Contains("W-8847"));
        Assert.Contains(items, i => (string?)i!["kind"] == "break" && ((string?)i!["detail"])!.Contains("$11.58M"));
        Assert.Contains(items, i => (string?)i!["kind"] == "warning" && ((string?)i!["title"])!.Contains("SWIFT Gateway"));
    }

    [Fact]
    public async Task Me_CarriesTheCallersCapabilityMatrix()
    {
        // Screens derive affordances (canApprove) from here — never from the
        // Admin-gated user directory.
        var me = await factory.CreateClientFor(Users.Counsel).GetJsonAsync("/api/me");
        Assert.Equal("approve", (string?)me["capabilities"]!["Approvals"]);
        Assert.Equal("none", (string?)me["capabilities"]!["Admin"]);
        Assert.Equal("view", (string?)me["capabilities"]!["Ref Data"]);
    }

    [Fact]
    public async Task Rbac_DeniesReadsOutsideTheCallersMatrix()
    {
        // Counsel: Wires none, Recon none, Admin none.
        var counsel = factory.CreateClientFor(Users.Counsel);
        Assert.Equal(HttpStatusCode.Forbidden, (await counsel.GetAsync("/api/wires")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await counsel.GetAsync("/api/reconciliation")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await counsel.GetAsync("/api/admin/users")).StatusCode);
    }
}
