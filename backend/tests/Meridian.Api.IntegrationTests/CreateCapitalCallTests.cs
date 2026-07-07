using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Meridian.Api.IntegrationTests.Support;
using Xunit;

namespace Meridian.Api.IntegrationTests;

public class CreateCapitalCallTests(MeridianApiFactory factory) : IClassFixture<MeridianApiFactory>
{
    private static object ValidRequest(decimal amount = 12.30m) => new
    {
        dealId = "deal-atlas",
        amount,
        dueDate = "2026-07-20",
        basis = "unfunded",
        allocations = new[]
        {
            new { investorId = "inv-redwood", amount = amount / 2 },
            new { investorId = "inv-blueharbor", amount = amount / 2 },
        },
    };

    [Fact]
    public async Task Create_LandsAtStageOne_WithNoticesQueuedAndAudit()
    {
        var client = factory.CreateClientFor(Users.OpsAnalyst);
        var response = await client.PostAsJsonAsync("/api/capital-calls", ValidRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var call = await response.ReadJsonAsync();
        Assert.NotNull(response.Headers.Location);
        Assert.EndsWith((string)call["id"]!, response.Headers.Location!.ToString());

        Assert.Equal(1, (int?)call["currentStage"]);
        Assert.Equal("In Review", (string?)call["status"]);
        Assert.Equal("unfunded", (string?)call["basis"]);
        Assert.Equal("fund-iii", (string?)call["fundId"]);
        Assert.Empty(call["pendingEscalations"]!.AsArray());
        Assert.Contains(call["documents"]!.AsArray(), d => (string?)d!["name"] == "Capital Call Notice.pdf");
        Assert.Contains(call["audit"]!.AsArray(), a => (string?)a!["title"] == "Call created");
        Assert.Contains(call["audit"]!.AsArray(), a => (string?)a!["title"] == "Notices queued");

        // Commitments come from the registry, not the request.
        var redwood = call["allocations"]!.AsArray().Single(a => (string?)a!["investorId"] == "inv-redwood")!;
        Assert.Equal(40m, (decimal?)redwood["commitment"]);
        Assert.Equal("Pending", (string?)redwood["wireStatus"]);
    }

    [Fact]
    public async Task Create_AllocationsMustReconcileToCallAmount()
    {
        var response = await factory.CreateClientFor(Users.OpsAnalyst).PostAsJsonAsync("/api/capital-calls", new
        {
            dealId = "deal-atlas",
            amount = 12.30m,
            dueDate = "2026-07-20",
            basis = "unfunded",
            allocations = new[] { new { investorId = "inv-redwood", amount = 12.00m } },
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.ReadJsonAsync();
        Assert.Contains("reconcile", (string?)problem["detail"]);
    }

    [Fact]
    public async Task Create_UnknownDeal_Is400()
    {
        var response = await factory.CreateClientFor(Users.OpsAnalyst).PostAsJsonAsync("/api/capital-calls", new
        {
            dealId = "deal-nope",
            amount = 1.00m,
            dueDate = "2026-07-20",
            basis = "unfunded",
            allocations = new[] { new { investorId = "inv-redwood", amount = 1.00m } },
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_UnknownInvestor_Is400()
    {
        var response = await factory.CreateClientFor(Users.OpsAnalyst).PostAsJsonAsync("/api/capital-calls", new
        {
            dealId = "deal-atlas",
            amount = 1.00m,
            dueDate = "2026-07-20",
            basis = "unfunded",
            allocations = new[] { new { investorId = "inv-nope", amount = 1.00m } },
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvestorWithoutCommitmentInFund_Is400()
    {
        // Granite State only has a Fund II commitment; deal-atlas is Fund III.
        var response = await factory.CreateClientFor(Users.OpsAnalyst).PostAsJsonAsync("/api/capital-calls", new
        {
            dealId = "deal-atlas",
            amount = 1.00m,
            dueDate = "2026-07-20",
            basis = "unfunded",
            allocations = new[] { new { investorId = "inv-granite", amount = 1.00m } },
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.ReadJsonAsync();
        Assert.Contains("no commitment", (string?)problem["detail"]);
    }

    [Fact]
    public async Task Create_PastDueDate_Is400()
    {
        var response = await factory.CreateClientFor(Users.OpsAnalyst).PostAsJsonAsync("/api/capital-calls", new
        {
            dealId = "deal-atlas",
            amount = 1.00m,
            dueDate = "2026-07-01", // business date is pinned to 2026-07-05
            basis = "unfunded",
            allocations = new[] { new { investorId = "inv-redwood", amount = 1.00m } },
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_OverTwentyMillion_TriggersEscalation()
    {
        var response = await factory.CreateClientFor(Users.OpsAnalyst).PostAsJsonAsync("/api/capital-calls", new
        {
            dealId = "deal-atlas",
            amount = 30.00m,
            dueDate = "2026-07-25",
            basis = "commitment",
            allocations = new[]
            {
                new { investorId = "inv-redwood", amount = 15.00m },
                new { investorId = "inv-cascade", amount = 15.00m },
            },
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var call = await response.ReadJsonAsync();
        var pending = call["pendingEscalations"]!.AsArray().Select(n => (string?)n).ToList();
        Assert.Contains("CIO", pending);
        Assert.Contains("Compliance", pending);
        Assert.Contains(call["audit"]!.AsArray(), a =>
            ((string?)a!["title"])!.StartsWith("Escalation triggered", StringComparison.Ordinal));
    }
}
