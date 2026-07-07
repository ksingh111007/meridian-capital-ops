using System.Net;
using System.Net.Http.Json;
using Meridian.Api.IntegrationTests.Support;
using Xunit;

namespace Meridian.Api.IntegrationTests;

/// <summary>The capability matrix must gate every endpoint server-side — UI affordances are hints, not security.</summary>
public class RbacTests(MeridianApiFactory factory) : IClassFixture<MeridianApiFactory>
{
    [Fact]
    public async Task AuditLog_RequiresAdminView()
    {
        // Counsel has Admin: none.
        var forbidden = await factory.CreateClientFor(Users.Counsel).GetAsync("/api/admin/audit");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        // Compliance has Admin: view.
        var allowed = await factory.CreateClientFor(Users.Compliance).GetAsync("/api/admin/audit");
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
    }

    [Fact]
    public async Task CreateCall_RequiresBlotterEdit()
    {
        // CIO has Blotter: view only.
        var response = await factory.CreateClientFor(Users.Cio)
            .PostAsJsonAsync("/api/capital-calls", new { dealId = "deal-atlas", amount = 1, dueDate = "2026-08-01", basis = "unfunded" });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RunningJobs_RequiresAdminEdit()
    {
        // Ops Manager has Admin: view — may list jobs but not fire them.
        var opsManager = factory.CreateClientFor(Users.OpsManager);
        var list = await opsManager.GetAsync("/api/ops/jobs");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);

        var run = await opsManager.PostAsync("/api/ops/jobs/custodian-feed-sync/run", null);
        Assert.Equal(HttpStatusCode.Forbidden, run.StatusCode);
    }

    [Fact]
    public async Task CapabilityHierarchy_FullSatisfiesLowerLevels()
    {
        // Administrator (full everywhere) can read the blotter and the audit log.
        var admin = factory.CreateClientFor(Users.Admin);
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/api/capital-calls")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/api/admin/audit")).StatusCode);
    }
}
