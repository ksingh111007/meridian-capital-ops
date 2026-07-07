using System.Data;
using Meridian.Application.Abstractions;
using Microsoft.Data.SqlClient;

namespace Meridian.Infrastructure.Persistence;

/// <summary>
/// Dapper connections against Azure SQL / SQL Server. The schema is owned by the
/// database/ dacpac project; hand-written SQL qualifies tables via <see cref="Table"/>.
/// </summary>
public sealed class SqlServerConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public IDbConnection CreateOpenConnection()
    {
        var connection = new SqlConnection(connectionString);
        connection.Open();
        return connection;
    }

    public string Table(string schema, string name) => $"[{schema}].[{name}]";
}
