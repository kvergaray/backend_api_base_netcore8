using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System.Data.Common;

namespace backend_api_base_netcore8.Infrastructure.Data;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseOptions _options;

    public DbConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured.");
        }
    }

    public DatabaseProvider Provider => _options.Provider;

    public async Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }

    private DbConnection CreateConnection() =>
        _options.Provider switch
        {
            DatabaseProvider.MySql => new MySqlConnection(_options.ConnectionString),
            DatabaseProvider.SqlServer => new SqlConnection(_options.ConnectionString),
            DatabaseProvider.PostgreSql => new NpgsqlConnection(_options.ConnectionString),
            DatabaseProvider.Oracle => new OracleConnection(_options.ConnectionString),
            _ => throw new InvalidOperationException($"Unsupported database provider '{_options.Provider}'.")
        };
}
