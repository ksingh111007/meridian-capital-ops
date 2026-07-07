namespace Meridian.Application.Abstractions;

/// <summary>
/// Database schema names — must match the database/ SQL project. SQLite has no
/// schemas (the dev provider ignores them); on Azure SQL every object lives in
/// one of these, never dbo.
/// </summary>
public static class DbSchemas
{
    /// <summary>Master/reference data: funds, deals, investors, borrowers, currencies.</summary>
    public const string Ref = "ref";

    /// <summary>Transactional fund operations: calls, distributions, wires, recon, treasury.</summary>
    public const string Ops = "ops";

    /// <summary>Staff, roles, and platform configuration.</summary>
    public const string Admin = "admin";

    /// <summary>The append-only, hash-chained audit log.</summary>
    public const string Audit = "audit";

    /// <summary>Investor-portal identities, capital accounts, and documents.</summary>
    public const string Portal = "portal";

    /// <summary>Temporal history tables (one per base table), managed by SQL Server.</summary>
    public const string History = "hist";
}
