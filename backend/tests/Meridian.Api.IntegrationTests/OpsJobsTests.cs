using System.Net;
using System.Text.Json.Nodes;
using Meridian.Api.IntegrationTests.Support;
using Meridian.Domain;
using Meridian.Domain.Entities;
using Meridian.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Meridian.Api.IntegrationTests;

public class OpsJobsTests(MeridianApiFactory factory) : IClassFixture<MeridianApiFactory>
{
    [Fact]
    public async Task ListJobs_ShowsTheThreeAutomationJobs_WithoutCronInTests()
    {
        var jobs = (await factory.CreateClientFor(Users.Admin).GetJsonAsync("/api/ops/jobs")).AsArray();
        var names = jobs.Select(j => (string?)j!["name"]).ToList();
        Assert.Contains("overdue-allocation-sweep", names);
        Assert.Contains("approval-sla-monitor", names);
        Assert.Contains("custodian-feed-sync", names);
        Assert.All(jobs, j => Assert.False((bool?)j!["hasCronTrigger"]));
    }

    [Fact]
    public async Task RunUnknownJob_Is404()
    {
        var response = await factory.CreateClientFor(Users.Admin).PostAsync("/api/ops/jobs/not-a-job/run", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OverdueSweep_FlipsPastDuePendingAllocations_EndToEnd()
    {
        // Arrange: a call past its due date with an unpaid allocation (setup writes
        // through the real DbContext; everything after goes through HTTP + Quartz).
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.CapitalCalls.Add(new CapitalCall
            {
                Id = "call-9001", Ref = "#C-9001", DealId = "deal-delta", DealName = "Project Delta",
                FundId = "fund-ii", Tranche = "Term B", Borrower = "Delta Logistics",
                Amount = 5m, DueDate = new DateOnly(2026, 7, 1), CurrentStage = 6, Status = CallStatus.Pending,
                Allocations =
                [
                    new CallAllocation { InvestorId = "inv-granite", InvestorName = "Granite State Insurance", Commitment = 35m, Amount = 5m, WireStatus = WireStatus.Pending },
                ],
            });
            await db.SaveChangesAsync();
        }

        var trigger = await factory.CreateClientFor(Users.Admin).PostAsync("/api/ops/jobs/overdue-allocation-sweep/run", null);
        Assert.Equal(HttpStatusCode.Accepted, trigger.StatusCode);

        // The job runs on the Quartz scheduler thread — poll until it lands.
        var client = factory.CreateClientFor(Users.Admin);
        string? wireStatus = null;
        for (var attempt = 0; attempt < 60; attempt++)
        {
            var call = await client.GetJsonAsync("/api/capital-calls/call-9001");
            wireStatus = (string?)call["allocations"]!.AsArray()[0]!["wireStatus"];
            if (wireStatus == "Overdue")
                break;
            await Task.Delay(250);
        }

        Assert.Equal("Overdue", wireStatus);

        // The sweep audited and notified.
        var log = await factory.CreateClientFor(Users.Compliance).GetJsonAsync("/api/admin/audit");
        Assert.Contains(log["events"]!.AsArray(), e =>
            (string?)e!["action"] == "Overdue" && ((string?)e!["object"])!.Contains("#C-9001"));
        Assert.True((bool?)log["kpis"]!["chainValid"]);

        using var verifyScope = factory.Services.CreateScope();
        var notifications = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>().Notifications;
        Assert.True(await notifications.AnyAsync(n => n.Subject.Contains("#C-9001")));
    }
}
