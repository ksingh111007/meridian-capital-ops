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

    /// <summary>Marks a portal-contact principal; carries the linked investor id.</summary>
    public const string InvestorIdClaim = "investorId";
    public const string PortalRoleClaim = "portalRole";
    public const string PortalRole = "PortalContact";
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
        if (user is not null)
        {
            if (user.Status == "Disabled")
                return AuthenticateResult.Fail("Unknown or disabled user.");

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

        // Not staff — the id may be a portal contact (the LP-portal dev stand-in,
        // replaced by real portal auth alongside staff SSO). Disabled contacts
        // cannot sign in (BACKEND_TODO acceptance criteria).
        var contact = await db.PortalContacts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == userId);
        if (contact is null || contact.Status == "Disabled")
            return AuthenticateResult.Fail("Unknown or disabled user.");

        var portalIdentity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, contact.Id),
            new Claim(ClaimTypes.Name, contact.Name),
            new Claim(ClaimTypes.Role, HeaderAuthentication.PortalRole),
            new Claim(HeaderAuthentication.InitialsClaim, contact.Initials),
            new Claim(HeaderAuthentication.InvestorIdClaim, contact.InvestorId),
            new Claim(HeaderAuthentication.PortalRoleClaim, contact.Role),
        ], Scheme.Name);

        return AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(portalIdentity), Scheme.Name));
    }
}
