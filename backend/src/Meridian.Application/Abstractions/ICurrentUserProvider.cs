using Meridian.Domain;

namespace Meridian.Application.Abstractions;

public sealed record CurrentUser(
    string Id,
    string Name,
    string Initials,
    string RoleName,
    IReadOnlyDictionary<ModuleName, Capability> Capabilities)
{
    public bool HasAtLeast(ModuleName module, Capability capability) =>
        Capabilities.TryGetValue(module, out var granted) && granted >= capability;
}

/// <summary>Resolves the authenticated caller and their role's capability matrix.</summary>
public interface ICurrentUserProvider
{
    Task<CurrentUser> GetRequiredAsync(CancellationToken cancellationToken = default);
}
