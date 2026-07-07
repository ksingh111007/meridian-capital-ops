using System.Security.Claims;
using Meridian.Application.Abstractions;
using Meridian.Domain;

namespace Meridian.Api.Auth;

/// <summary>
/// Resolves the portal session from the authenticated portal-contact principal.
/// Staff principals (or anonymous callers) get Forbidden — portal data is only
/// ever scoped to the LP behind the session.
/// </summary>
public sealed class HttpPortalSessionProvider(IHttpContextAccessor httpContextAccessor) : IPortalSessionProvider
{
    public Task<PortalSession> GetRequiredAsync(CancellationToken cancellationToken = default)
    {
        var principal = httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
            throw DomainException.Forbidden("No authenticated user.");

        var investorId = principal.FindFirstValue(HeaderAuthentication.InvestorIdClaim);
        if (string.IsNullOrEmpty(investorId))
            throw DomainException.Forbidden("A portal-contact session is required for portal endpoints.");

        return Task.FromResult(new PortalSession(
            principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
            principal.FindFirstValue(ClaimTypes.Name) ?? "",
            principal.FindFirstValue(HeaderAuthentication.InitialsClaim) ?? "",
            investorId,
            principal.FindFirstValue(HeaderAuthentication.PortalRoleClaim) ?? "Viewer"));
    }
}
