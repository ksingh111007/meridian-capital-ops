using System.Net;
using Meridian.Api.IntegrationTests.Support;
using Xunit;

namespace Meridian.Api.IntegrationTests;

public class AuthTests(MeridianApiFactory factory) : IClassFixture<MeridianApiFactory>
{
    [Fact]
    public async Task MissingUserHeader_Is401()
    {
        var response = await factory.CreateClient().GetAsync("/api/capital-calls");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UnknownUser_Is401()
    {
        var response = await factory.CreateClientFor("u-ghost").GetAsync("/api/capital-calls");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task InvitedUser_CannotAuthenticate()
    {
        // u-akim has never accepted their account (Status = "Invited") — only
        // Active staff may act; an invitee must not be able to read or approve.
        var response = await factory.CreateClientFor("u-akim").GetAsync("/api/capital-calls");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ReturnsSessionUserShape()
    {
        var me = await factory.CreateClientFor(Users.Counsel).GetJsonAsync("/api/me");
        Assert.Equal("u-jokafor", (string?)me["id"]);
        Assert.Equal("J. Okafor", (string?)me["name"]);
        Assert.Equal("JO", (string?)me["initials"]);
        Assert.Equal("Counsel", (string?)me["role"]);
    }

    [Fact]
    public async Task HealthCheck_IsAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/healthz");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
