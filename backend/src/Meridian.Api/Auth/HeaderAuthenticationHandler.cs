using System.Security.Claims;
using System.Text.Encodings.Web;
using Meridian.Application.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Meridian.Api.Auth;

public static class HeaderAuthentication
{
    public const string Scheme = "MeridianHeader";
    public const string UserIdHeader = "X-User-Id";
    public const string InitialsClaim = "initials";
}

/// <summary>
/// Development stand-in for staff SSO: the caller identifies as a seeded staff
/// user via the X-User-Id header. Deliberately isolated behind the standard
/// ASP.NET authentication pipeline — swapping to OIDC (Entra ID) replaces this
/// one handler registration and nothing else. Never ship this scheme to
/// production; RBAC below it is real either way.
/// </summary>
public sealed class HeaderAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderAuthentication.UserIdHeader, out var values)
            || values.FirstOrDefault() is not { Length: > 0 } userId)
        {
            return AuthenticateResult.NoResult();
        }

        var db = Context.RequestServices.GetRequiredService<IAppDbContext>();
        var user = await db.StaffUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        // Only Active staff may act: Invited users have not accepted their account
        // yet and Disabled users are locked out — neither may authenticate.
        if (user is null || user.Status != "Active")
            return AuthenticateResult.Fail("Unknown or inactive user.");

        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.RoleName),
            new Claim(HeaderAuthentication.InitialsClaim, user.Initials),
        ], Scheme.Name);

        return AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name));
    }
}
