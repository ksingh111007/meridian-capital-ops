using Meridian.Application.Abstractions;
using Meridian.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Application.Admin;

public sealed record UserKpisDto(int Users, int Active, int PendingInvites, int Roles);

public sealed record StaffUserDto(
    string Id, string Name, string Initials, string Email, string Role, string FundAccess,
    string LastActive, string Status);

public sealed record RoleDto(string Name, IReadOnlyDictionary<string, string> Capabilities);

public sealed record UsersAndRolesDto(UserKpisDto Kpis, IReadOnlyList<StaffUserDto> Users, IReadOnlyList<RoleDto> Roles);

public class AdminDirectoryService(IAppDbContext db)
{
    public async Task<UsersAndRolesDto> GetUsersAndRolesAsync(CancellationToken ct = default)
    {
        var kpis = await KpiReader.ForScreenAsync(db, "users", ct);
        var users = await db.StaffUsers.AsNoTracking().OrderBy(u => u.Id).ToListAsync(ct);
        var roles = await db.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync(ct);

        return new UsersAndRolesDto(
            new UserKpisDto(kpis.Count("users"), kpis.Count("active"), kpis.Count("pendingInvites"), kpis.Count("roles")),
            users.Select(u => new StaffUserDto(
                u.Id, u.Name, u.Initials, u.Email, u.RoleName, u.FundAccess, u.LastActive, u.Status)).ToList(),
            roles.Select(r => new RoleDto(
                r.Name,
                r.Capabilities.ToDictionary(c => c.Key.ToDisplay(), c => c.Value.ToDisplay()))).ToList());
    }
}
