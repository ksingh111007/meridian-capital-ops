using System.Data;
using Meridian.Application.Abstractions;
using Microsoft.Data.Sqlite;

namespace Meridian.Infrastructure.Persistence;

/// <summary>
/// Shared-cache in-memory SQLite. The singleton holds one open keep-alive
/// connection so the database survives while the app runs; EF Core and Dapper
/// each open ordinary connections against the same shared cache.
/// </summary>
public sealed class SqliteConnectionFactory : IDbConnectionFactory, IDisposable
{
    private readonly string _connectionString;
    private readonly SqliteConnection _keepAlive;

    public SqliteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
        _keepAlive = new SqliteConnection(connectionString);
        _keepAlive.Open();
    }

    public string ConnectionString => _connectionString;

    public IDbConnection CreateOpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    /// <summary>SQLite has no schemas — tables go by their bare name.</summary>
    public string Table(string schema, string name) => $"\"{name}\"";

    public void Dispose() => _keepAlive.Dispose();
}
