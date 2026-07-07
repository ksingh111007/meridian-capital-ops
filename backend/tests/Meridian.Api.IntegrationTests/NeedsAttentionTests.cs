using System.Text.Json.Nodes;
using Meridian.Api.IntegrationTests.Support;
using Xunit;

namespace Meridian.Api.IntegrationTests;

/// <summary>Exercises the Dapper-computed inbox against the seeded story (business date 2026-07-05).</summary>
public class NeedsAttentionTests(MeridianApiFactory factory) : IClassFixture<MeridianApiFactory>
{
    [Fact]
    public async Task Counsel_SeesTheirPendingApproval_AsMine()
    {
        var items = (await factory.CreateClientFor(Users.Counsel).GetJsonAsync("/api/needs-attention")).AsArray();

        var mine = items.Single(i => (string?)i!["kind"] == "approval" && (bool?)i!["mine"] == true)!;
        Assert.Contains("#C-2041", (string?)mine["title"]);
        Assert.Contains("Legal", (string?)mine["title"]);
        Assert.Equal("/capital-calls/call-2041", (string?)mine["href"]);
        Assert.Contains("you are the stage approver", (string?)mine["detail"]);
    }

    [Fact]
    public async Task OverdueWires_OnCall2039_SurfaceWithAging()
    {
        var items = (await factory.CreateClientFor(Users.Counsel).GetJsonAsync("/api/needs-attention")).AsArray();

        var overdue = items.Single(i => (string?)i!["kind"] == "overdue")!;
        Assert.Equal("2 LP wires overdue on Call #C-2039", (string?)overdue["title"]);
        Assert.Contains("$7.10M", (string?)overdue["detail"]);
        Assert.Contains("(4d late)", (string?)overdue["detail"]); // due Jul 01, today Jul 05
        Assert.Equal("red", (string?)overdue["tone"]);
    }

    [Fact]
    public async Task ReturnedCall_AndDueSoonCall_AreVisibleToEveryone()
    {
        var items = (await factory.CreateClientFor(Users.OpsAnalyst).GetJsonAsync("/api/needs-attention")).AsArray();

        Assert.Contains(items, i => (string?)i!["kind"] == "approval"
            && ((string?)i!["title"])!.Contains("#C-2039")
            && ((string?)i!["title"])!.Contains("returned to CIO"));
        Assert.Contains(items, i => (string?)i!["kind"] == "due-soon"
            && ((string?)i!["title"])!.Contains("#C-2041")
            && ((string?)i!["title"])!.Contains("due Jul 08"));
    }

    [Fact]
    public async Task ApprovalItems_AreScopedToTheCallersRole()
    {
        var items = (await factory.CreateClientFor(Users.OpsAnalyst).GetJsonAsync("/api/needs-attention")).AsArray();
        Assert.DoesNotContain(items, i => (bool?)i!["mine"] == true && ((string?)i!["title"])!.Contains("#C-2041"));
    }
}
