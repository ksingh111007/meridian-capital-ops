namespace Meridian.Domain.Entities;

public class StaffUser
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Initials { get; set; } = "";
    public string Email { get; set; } = "";
    public string RoleName { get; set; } = "";
    public string FundAccess { get; set; } = "All funds";
    public string Status { get; set; } = "Active";
}

/// <summary>The role → capability matrix is the single RBAC source of truth (BUSINESS_RULES.md).</summary>
public class Role
{
    public string Name { get; set; } = "";
    public Dictionary<ModuleName, Capability> Capabilities { get; set; } = [];
}
