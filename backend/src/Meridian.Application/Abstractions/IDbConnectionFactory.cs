using System.Data;

namespace Meridian.Application.Abstractions;

/// <summary>
/// Raw ADO.NET connections for Dapper. Reserved for read paths where hand-written
/// SQL beats materializing aggregates through the ORM (dashboards, computed inboxes).
/// Writes always go through EF Core so change tracking and auditing stay intact.
/// </summary>
public interface IDbConnectionFactory
{
    IDbConnection CreateOpenConnection();
}
