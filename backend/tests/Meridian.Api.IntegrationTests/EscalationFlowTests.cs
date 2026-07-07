using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Meridian.Api.IntegrationTests.Support;
using Xunit;

namespace Meridian.Api.IntegrationTests;

/// <summary>
/// End-to-end escalation: a call over $20M requires CIO + Compliance sign-off and
/// cannot pass the CIO stage until both have landed (BUSINESS_RULES.md).
/// </summary>
public class EscalationFlowTests(MeridianApiFactory factory) : IClassFixture<MeridianApiFactory>
{
    [Fact]
    public async Task LargeCall_IsGatedAtCio_UntilComplianceSignsOff()
    {
        var create = await factory.CreateClientFor(Users.OpsAnalyst).PostAsJsonAsync("/api/capital-calls", new
        {
            dealId = "deal-atlas",
            amount = 25.00m,
            dueDate = "2026-07-28",
            basis = "unfunded",
            allocations = new[]
            {
                new { investorId = "inv-redwood", amount = 10.00m },
                new { investorId = "inv-cascade", amount = 15.00m },
            },
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var id = (string)(await create.ReadJsonAsync())["id"]!;

        // Stages 1 and 2 approve normally.
        foreach (var user in new[] { Users.OpsAnalyst, Users.DealLead })
        {
            var ok = await factory.CreateClientFor(user)
                .PostAsJsonAsync($"/api/capital-calls/{id}/approve", new { comment = "ok" });
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        }

        // CIO cannot push the call past their stage while Compliance is outstanding.
        var blocked = await factory.CreateClientFor(Users.Cio)
            .PostAsJsonAsync($"/api/capital-calls/{id}/approve", new { comment = "proceed" });
        Assert.Equal(HttpStatusCode.Conflict, blocked.StatusCode);
        Assert.Contains("Compliance", (string?)(await blocked.ReadJsonAsync())["detail"]);

        // Compliance signs off — no stage movement.
        var signoff = await factory.CreateClientFor(Users.Compliance)
            .PostAsJsonAsync($"/api/capital-calls/{id}/approve", new { comment = "Reviewed under policy 7.3" });
        Assert.Equal(HttpStatusCode.OK, signoff.StatusCode);
        var signoffResult = await signoff.ReadJsonAsync();
        Assert.True((bool?)signoffResult["escalationSignoff"]);
        Assert.Equal(3, (int?)signoffResult["call"]!["currentStage"]);

        // Now the CIO stage approval advances to Legal with no pending escalations left.
        var approved = await factory.CreateClientFor(Users.Cio)
            .PostAsJsonAsync($"/api/capital-calls/{id}/approve", new { comment = "proceed" });
        Assert.Equal(HttpStatusCode.OK, approved.StatusCode);
        var result = await approved.ReadJsonAsync();
        Assert.Equal(4, (int?)result["call"]!["currentStage"]);
        Assert.Empty(result["call"]!["pendingEscalations"]!.AsArray());
    }
}
