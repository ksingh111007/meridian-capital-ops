namespace Meridian.Application.Abstractions;

/// <param name="Role">Primary | Viewer | Tax-only.</param>
public sealed record PortalSession(
    string ContactId,
    string ContactName,
    string ContactInitials,
    string InvestorId,
    string Role);

/// <summary>
/// Resolves the investor-portal session. Portal endpoints derive the investor from
/// this — never from a request parameter (docs/API.md portal scoping rule).
/// </summary>
public interface IPortalSessionProvider
{
    Task<PortalSession> GetRequiredAsync(CancellationToken cancellationToken = default);
}
