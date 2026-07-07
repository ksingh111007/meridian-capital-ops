using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Meridian.Api.IntegrationTests.Support;
using Xunit;

namespace Meridian.Api.IntegrationTests;

/// <summary>
/// Portal endpoints are scoped to the LP behind the session (never a parameter):
/// Karen Doyle (pc-1, Primary) → Redwood Pension. Disabled contacts cannot sign
/// in; Tax-only contacts see only tax + IR; staff principals are rejected.
/// </summary>
public class PortalTests(MeridianApiFactory factory) : IClassFixture<MeridianApiFactory>
{
    private const string PrimaryContact = "pc-1";  // Karen Doyle — Redwood Pension
    private const string TaxOnlyContact = "pc-4";  // t.wells — Granite State, Tax-only (Invited)
    private const string DisabledContact = "pc-6"; // R. Okafor — Oakmont, Disabled

    [Fact]
    public async Task Account_IsScopedToTheSessionInvestor()
    {
        var account = await factory.CreateClientFor(PrimaryContact).GetJsonAsync("/api/portal/account");

        Assert.Equal("inv-redwood", (string?)account["investorId"]);
        Assert.Equal("Karen Doyle", (string?)account["contactName"]);
        Assert.Equal(58m, (decimal?)account["stats"]!["commitment"]);
        Assert.Equal(2, account["funds"]!.AsArray().Count);
    }

    [Fact]
    public async Task Investments_CarryPositionsTotalsAndRollforward()
    {
        var investments = await factory.CreateClientFor(PrimaryContact).GetJsonAsync("/api/portal/investments");

        Assert.Equal(2, investments["positions"]!.AsArray().Count);
        Assert.Equal(1.30m, (decimal?)investments["totals"]!["tvpi"]);
        var lines = investments["rollforward"]!["lines"]!.AsArray();
        Assert.Equal(5, lines.Count);
        Assert.Equal("Q2 2026", (string?)investments["rollforward"]!["period"]);
        // Contributions hit Fund III only — the Fund II cell is null.
        var contributions = lines.Single(l => (string?)l!["line"] == "Contributions")!;
        Assert.Null(contributions["fundII"]);
    }

    [Fact]
    public async Task Activity_MirrorsTheInternalStory()
    {
        var activity = await factory.CreateClientFor(PrimaryContact).GetJsonAsync("/api/portal/activity");

        var due = activity["rows"]!.AsArray().Single(r => (string?)r!["reference"] == "#C-2041")!;
        Assert.Equal(-8.2m, (decimal?)due["amount"]);
        Assert.Equal("Due", (string?)due["status"]);
        Assert.Equal("2026-07-08", (string?)activity["stats"]!["nextCallDue"]);
    }

    [Fact]
    public async Task StatementsAndTax_ServeTheDocumentLibrary()
    {
        var client = factory.CreateClientFor(PrimaryContact);

        var statements = await client.GetJsonAsync("/api/portal/statements");
        Assert.Equal(34, (int?)statements["totalCount"]);
        Assert.Equal(8, statements["documents"]!.AsArray().Count);

        var tax = await client.GetJsonAsync("/api/portal/tax");
        var pending = tax["documents"]!.AsArray().Single(d => (string?)d!["status"] == "Pending")!;
        Assert.Equal("Mar 2027", (string?)pending["expectedDate"]);
    }

    [Fact]
    public async Task ContactMessage_CreatesATicketedRequest()
    {
        var client = factory.CreateClientFor(PrimaryContact);

        var response = await client.PostAsJsonAsync("/api/portal/messages",
            new { subject = "Wire instructions update", regarding = "Wire instructions", message = "Please update our account." });
        var ticket = await response.ReadJsonAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("#REQ-", (string?)ticket["ref"]);
        Assert.Equal("Open", (string?)ticket["status"]);

        var contact = await client.GetJsonAsync("/api/portal/contact");
        Assert.Contains(contact["recentRequests"]!.AsArray(),
            r => (string?)r!["subject"] == "Wire instructions update");
    }

    [Fact]
    public async Task DisabledContacts_CannotSignIn()
    {
        var response = await factory.CreateClientFor(DisabledContact).GetAsync("/api/portal/account");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Session_IsAvailableToEveryPortalContact()
    {
        // The shell renders from the session — even for contacts who cannot
        // read the capital account (Tax-only).
        var session = await factory.CreateClientFor(TaxOnlyContact).GetJsonAsync("/api/portal/session");
        Assert.Equal("inv-granite", (string?)session["investorId"]);
        Assert.Equal("Granite State Insurance", (string?)session["investor"]);
        Assert.Equal("Tax-only", (string?)session["role"]);
    }

    [Fact]
    public async Task TaxOnlyContacts_AreLimitedToTaxAndIr()
    {
        var client = factory.CreateClientFor(TaxOnlyContact);

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/portal/account")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/portal/activity")).StatusCode);

        var tax = await client.GetJsonAsync("/api/portal/tax");
        Assert.NotNull(tax["banner"]);

        // Granite has no seeded statements; a tax-statements contact sees only Tax docs anyway.
        var statements = await client.GetJsonAsync("/api/portal/statements");
        Assert.All(statements["documents"]!.AsArray(), d => Assert.Equal("Tax", (string?)d!["type"]));
    }

    [Fact]
    public async Task StaffPrincipals_CannotUsePortalEndpoints()
    {
        var response = await factory.CreateClientFor(Users.Admin).GetAsync("/api/portal/account");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
