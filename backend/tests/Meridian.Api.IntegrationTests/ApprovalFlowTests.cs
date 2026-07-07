using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Meridian.Api.IntegrationTests.Support;
using Xunit;

namespace Meridian.Api.IntegrationTests;

public class ApprovalFlowTests(MeridianApiFactory factory) : IClassFixture<MeridianApiFactory>
{
    private async Task<string> CreateCallAsync(decimal amount = 8.40m)
    {
        var response = await factory.CreateClientFor(Users.OpsAnalyst).PostAsJsonAsync("/api/capital-calls", new
        {
            dealId = "deal-beacon",
            amount,
            dueDate = "2026-07-30",
            basis = "unfunded",
            allocations = new[]
            {
                new { investorId = "inv-summit", amount = amount / 2 },
                new { investorId = "inv-ironwood", amount = amount / 2 },
            },
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (string)(await response.ReadJsonAsync())["id"]!;
    }

    private Task<HttpResponseMessage> ActAsync(string userId, string callId, string action, string? comment) =>
        factory.CreateClientFor(userId).PostAsJsonAsync($"/api/capital-calls/{callId}/{action}", new { comment });

    [Fact]
    public async Task StoryCall2041_CounselApproval_AdvancesToOpsFinalReview()
    {
        var response = await ActAsync(Users.Counsel, "call-2041", "approve", "LPA §4.2 reviewed — clear to proceed");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.ReadJsonAsync();
        Assert.False((bool?)result["completed"]);
        Assert.Equal("Ops Manager", (string?)result["nextApproverRole"]);
        Assert.Equal(5, (int?)result["call"]!["currentStage"]);
        var stage4 = result["call"]!["stageEvents"]!.AsArray().Single(e => (int?)e!["stage"] == 4)!;
        Assert.Equal("done", (string?)stage4["state"]);
        Assert.Equal("LPA §4.2 reviewed — clear to proceed", (string?)stage4["comment"]);
        Assert.Equal("Legal approved", (string?)result["call"]!["audit"]!.AsArray().First()!["title"]);
    }

    [Fact]
    public async Task FullPipeline_SixManualApprovals_AutoStages_ThenCompleted()
    {
        var id = await CreateCallAsync();
        (string User, string Comment)[] chain =
        [
            (Users.OpsAnalyst, "Allocations verified"),
            (Users.DealLead, "Deal terms confirmed"),
            (Users.Cio, "Cleared to proceed"),
            (Users.Counsel, "LPA compliant"),
            (Users.OpsManager, "Final review done"),
            (Users.FundAccountant, "Booked"),
        ];

        foreach (var (user, comment) in chain)
        {
            var response = await ActAsync(user, id, "approve", comment);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        var call = await factory.CreateClientFor(Users.OpsAnalyst).GetJsonAsync($"/api/capital-calls/{id}");
        Assert.Equal("Completed", (string?)call["status"]);
        Assert.Equal(9, (int?)call["currentStage"]);
        // Stages 7 and 8 auto-advanced by the system; the terminal stage 9 gets its
        // own done event and the trail records completion (mock #C-2036 parity).
        var events = call["stageEvents"]!.AsArray();
        Assert.Equal("System", (string?)events.Single(e => (int?)e!["stage"] == 7)!["actor"]);
        Assert.Equal("done", (string?)events.Single(e => (int?)e!["stage"] == 8)!["state"]);
        Assert.Equal("done", (string?)events.Single(e => (int?)e!["stage"] == 9)!["state"]);
        Assert.Contains(call["audit"]!.AsArray(), a => (string?)a!["title"] == "Call completed");

        // The global audit log recorded the auto stages and the completion.
        var log = await factory.CreateClientFor(Users.Compliance).GetJsonAsync("/api/admin/audit");
        var logEvents = log["events"]!.AsArray();
        Assert.Contains(logEvents, e => (string?)e!["action"] == "Stage completed"
            && ((string?)e!["object"])!.Contains("Custodians Notified"));
        Assert.Contains(logEvents, e => (string?)e!["action"] == "Completed");
        Assert.True((bool?)log["kpis"]!["chainValid"]);
    }

    [Fact]
    public async Task Approve_WithoutComment_Is400()
    {
        var id = await CreateCallAsync();
        var response = await ActAsync(Users.OpsAnalyst, id, "approve", null);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.ReadJsonAsync();
        Assert.Contains("comment", ((string?)problem["detail"])!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Approve_ByWrongStageRole_Is403()
    {
        var id = await CreateCallAsync(); // stage 1 → Ops Analyst
        var response = await ActAsync(Users.Counsel, id, "approve", "not my stage");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Reject_ReturnsToPriorStage_AndFirstStageRejectConflicts()
    {
        var id = await CreateCallAsync();
        Assert.Equal(HttpStatusCode.OK, (await ActAsync(Users.OpsAnalyst, id, "approve", "on to front office")).StatusCode);

        var reject = await ActAsync(Users.DealLead, id, "reject", "Allocation looks off");
        Assert.Equal(HttpStatusCode.OK, reject.StatusCode);
        var result = await reject.ReadJsonAsync();
        Assert.Equal("Returned", (string?)result["call"]!["status"]);
        Assert.Equal(1, (int?)result["call"]!["currentStage"]);

        // Back at stage 1 the call can only be reworked, not rejected further.
        var conflict = await ActAsync(Users.OpsAnalyst, id, "reject", "cannot go lower");
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
    }

    [Fact]
    public async Task Approve_CompletedCall_Is409()
    {
        var response = await ActAsync(Users.Admin, "call-2036", "approve", "already done");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
