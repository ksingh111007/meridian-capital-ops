using System.Net;
using System.Text.Json.Nodes;
using Meridian.Api.IntegrationTests.Support;
using Xunit;

namespace Meridian.Api.IntegrationTests;

/// <summary>Server-side query composition over /odata/Deals (filter/order/top/count → SQL via EF).</summary>
public class ODataDealsTests(MeridianApiFactory factory) : IClassFixture<MeridianApiFactory>
{
    [Fact]
    public async Task Top_LimitsPageSize()
    {
        var page = await factory.CreateClientFor(Users.Counsel).GetJsonAsync("/odata/Deals?$top=3");
        Assert.Equal(3, page["value"]!.AsArray().Count);
    }

    [Fact]
    public async Task Filter_ByFund_ReturnsOnlyMatchingDeals()
    {
        var page = await factory.CreateClientFor(Users.Counsel)
            .GetJsonAsync("/odata/Deals?$filter=fundId eq 'fund-iii'&$count=true");
        var deals = page["value"]!.AsArray();
        Assert.NotEmpty(deals);
        Assert.All(deals, d => Assert.Equal("fund-iii", (string?)d!["fundId"]));
        Assert.Contains(deals, d => (string?)d!["id"] == "deal-atlas");
    }

    [Fact]
    public async Task OrderBy_InvestedDescending_IsSorted()
    {
        var page = await factory.CreateClientFor(Users.Counsel)
            .GetJsonAsync("/odata/Deals?$orderby=invested desc&$top=10");
        var invested = page["value"]!.AsArray().Select(d => (decimal)d!["invested"]!).ToList();
        Assert.Equal(invested.OrderByDescending(v => v), invested);
    }

    [Fact]
    public async Task Count_IncludesStoryAndFakeDeals()
    {
        var page = await factory.CreateClientFor(Users.Counsel).GetJsonAsync("/odata/Deals?$count=true&$top=1");
        Assert.True((int?)page["@odata.count"] >= 14); // 4 story + 10 Bogus-generated
    }

    [Fact]
    public async Task OData_RequiresAuthentication()
    {
        var response = await factory.CreateClient().GetAsync("/odata/Deals");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
