using System.Net;
using System.Text.Json.Nodes;
using Meridian.Api.IntegrationTests.Support;
using Xunit;

namespace Meridian.Api.IntegrationTests;

public class CapitalCallReadTests(MeridianApiFactory factory) : IClassFixture<MeridianApiFactory>
{
    [Fact]
    public async Task List_ContainsStoryCalls_WithContractShape()
    {
        var calls = (await factory.CreateClientFor(Users.Counsel).GetJsonAsync("/api/capital-calls")).AsArray();
        var ids = calls.Select(c => (string?)c!["id"]).ToList();
        Assert.Contains("call-2041", ids);
        Assert.Contains("call-2039", ids);
        Assert.Contains("call-2036", ids);
    }

    [Fact]
    public async Task Get_Call2041_MatchesTheMockStory()
    {
        var call = await factory.CreateClientFor(Users.Counsel).GetJsonAsync("/api/capital-calls/call-2041");

        Assert.Equal("#C-2041", (string?)call["ref"]);
        Assert.Equal("Project Atlas", (string?)call["deal"]);
        Assert.Equal("fund-iii", (string?)call["fundId"]);
        Assert.Equal(16m, (decimal?)call["amount"]);
        Assert.Equal("2026-07-08", (string?)call["dueDate"]);
        Assert.Equal(4, (int?)call["currentStage"]);
        Assert.Equal("In Review", (string?)call["status"]);

        var allocations = call["allocations"]!.AsArray();
        Assert.Equal(3, allocations.Count);
        var blueHarbor = allocations.Single(a => (string?)a!["investorId"] == "inv-blueharbor")!;
        Assert.Equal("Wired", (string?)blueHarbor["wireStatus"]);

        var stage4 = call["stageEvents"]!.AsArray().Single(e => (int?)e!["stage"] == 4)!;
        Assert.Equal("current", (string?)stage4["state"]);
        Assert.Equal("Jul 04", (string?)stage4["date"]);

        var newestAudit = call["audit"]!.AsArray().First()!;
        Assert.Equal("Legal review started", (string?)newestAudit["title"]);
    }

    [Fact]
    public async Task Get_Call2039_IsReturnedWithOverdueAllocations()
    {
        var call = await factory.CreateClientFor(Users.Counsel).GetJsonAsync("/api/capital-calls/call-2039");
        Assert.Equal("Returned", (string?)call["status"]);
        Assert.Equal(3, (int?)call["currentStage"]);
        var overdue = call["allocations"]!.AsArray().Where(a => (string?)a!["wireStatus"] == "Overdue").ToList();
        Assert.Equal(2, overdue.Count);
    }

    [Fact]
    public async Task Get_UnknownId_Is404ProblemDetails()
    {
        var response = await factory.CreateClientFor(Users.Counsel).GetAsync("/api/capital-calls/call-0000");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.ReadJsonAsync();
        Assert.Equal(404, (int?)problem["status"]);
        Assert.Contains("call-0000", (string?)problem["detail"]);
    }

    [Fact]
    public async Task Workflow_ReturnsNineStagesAndEscalationRules()
    {
        var workflow = await factory.CreateClientFor(Users.Counsel).GetJsonAsync("/api/workflows/capital-calls");
        var stages = workflow["stages"]!.AsArray();
        Assert.Equal(9, stages.Count);
        Assert.Equal("Counsel", (string?)stages.Single(s => (int?)s!["order"] == 4)!["approverRole"]);
        Assert.True((bool?)stages.Single(s => (int?)s!["order"] == 9)!["terminal"]);

        // All three rules are configured, but only the amount rule is enforceable
        // today — the API must not present the others as silently effective.
        var rules = workflow["escalationRules"]!.AsArray();
        Assert.Equal(3, rules.Count);
        Assert.True((bool?)rules.Single(r => ((string?)r!["condition"])!.Contains("$20M"))!["enforced"]);
        Assert.False((bool?)rules.Single(r => (string?)r!["condition"] == "Cross-fund allocation")!["enforced"]);
    }
}
