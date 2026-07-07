using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Meridian.Api.IntegrationTests.Support;

/// <summary>
/// Boots the real API host per test class with an isolated shared-cache in-memory
/// database (unique name per factory), the business date pinned to the seeded
/// story's "today" (2026-07-05), and Quartz cron triggers disabled so background
/// jobs only run when a test fires them through the ops endpoint.
/// </summary>
public sealed class MeridianApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"test-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", $"DataSource={_databaseName};Mode=Memory;Cache=Shared");
        builder.UseSetting("BusinessDate", "2026-07-05");
        builder.UseSetting("Jobs:OverdueSweepCron", "");
        builder.UseSetting("Jobs:SlaMonitorCron", "");
        builder.UseSetting("Jobs:CustodianFeedSyncCron", "");
    }

    /// <summary>Client authenticated as a seeded staff user via the dev header scheme.</summary>
    public HttpClient CreateClientFor(string userId)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", userId);
        return client;
    }

    /// <summary>
    /// Intentionally keeps the host alive for the whole test run. Quartz bridges its
    /// internal logging through a process-global LogProvider pointing at whichever
    /// host registered last; disposing a host while a later test class boots its own
    /// would hand Quartz a disposed LoggerFactory. Hosts are per-class and cheap;
    /// scheduler threads are background threads, so process exit cleans everything up.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        // no-op by design — see remarks above
    }
}

/// <summary>Seeded staff users (see Infrastructure/Seeding/StorySeed).</summary>
public static class Users
{
    public const string OpsAnalyst = "u-jchen";       // Jordan Chen — stage 1
    public const string DealLead = "u-mreyes";        // Maria Reyes — stage 2
    public const string Cio = "u-spatel";             // Sanjay Patel — stage 3
    public const string Counsel = "u-jokafor";        // J. Okafor — stage 4
    public const string OpsManager = "u-talvarez";    // Tom Alvarez — stage 5
    public const string FundAccountant = "u-dwhitfield"; // Dana Whitfield — stage 6
    public const string Compliance = "u-pnair";       // Priya Nair — escalation sign-off
    public const string Admin = "u-admin";            // Avery Whitman — full everywhere
}
