using System.Text.Json.Nodes;
using Meridian.Api.IntegrationTests.Support;
using Xunit;

namespace Meridian.Api.IntegrationTests;

public class DistributionsTests(MeridianApiFactory factory) : IClassFixture<MeridianApiFactory>
{
    [Fact]
    public async Task List_ReturnsStoryDistributions()
    {
        var distributions = (await factory.CreateClientFor(Users.Counsel).GetJsonAsync("/api/distributions")).AsArray();
        var ids = distributions.Select(d => (string?)d!["id"]).ToList();
        Assert.Equal(["dist-119", "dist-118", "dist-117", "dist-116"], ids);
    }

    [Theory]
    [InlineData("dist-116")]
    [InlineData("dist-117")]
    [InlineData("dist-118")]
    [InlineData("dist-119")]
    public async Task WaterfallInvariants_HoldExactly(string id)
    {
        var d = await factory.CreateClientFor(Users.Counsel).GetJsonAsync($"/api/distributions/{id}");

        var distributable = (decimal)d["distributable"]!;
        var lpTotal = (decimal)d["lpTotal"]!;
        var gpTotal = (decimal)d["gpTotal"]!;
        var tiers = d["tiers"]!.AsArray();

        // Tiers exhaust the pool exactly and LP + GP = distributable (BUSINESS_RULES.md).
        Assert.Equal(distributable, tiers.Sum(t => (decimal)t!["distributed"]!));
        Assert.Equal(distributable, lpTotal + gpTotal);
        Assert.Equal(lpTotal, tiers.Sum(t => (decimal?)t!["lpShare"] ?? 0m));
        Assert.Equal(gpTotal, tiers.Sum(t => (decimal?)t!["gpShare"] ?? 0m));
        Assert.Equal(0m, (decimal)tiers.Last()!["poolLeft"]!);

        // Per-investor payouts sum to the LP total.
        Assert.Equal(lpTotal, d["payouts"]!.AsArray().Sum(p => (decimal)p!["amount"]!));
    }

    [Fact]
    public async Task Dist119_BlockedAndExceptionPayouts_MatchTheStory()
    {
        var d = await factory.CreateClientFor(Users.Counsel).GetJsonAsync("/api/distributions/dist-119");
        var payouts = d["payouts"]!.AsArray();

        // Oakmont has no wire instructions on file → Blocked, never a wire.
        var oakmont = payouts.Single(p => (string?)p!["investorId"] == "inv-oakmont")!;
        Assert.Equal("Blocked", (string?)oakmont["status"]);
        Assert.Equal("No wire instructions on file", (string?)oakmont["blockedReason"]);
        Assert.Null(oakmont["wireRef"]);

        // Granite State's SWIFT wire W-8847 failed → actionable Exception.
        var granite = payouts.Single(p => (string?)p!["investorId"] == "inv-granite")!;
        Assert.Equal("Exception", (string?)granite["status"]);
        Assert.Equal("W-8847", (string?)granite["wireRef"]);
    }
}
