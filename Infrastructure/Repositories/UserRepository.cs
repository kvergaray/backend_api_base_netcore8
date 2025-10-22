using backend_api_base_netcore8.Application.Interfaces;
using backend_api_base_netcore8.Domain.Entities;
using backend_api_base_netcore8.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace backend_api_base_netcore8.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private const string SelectColumns = """
id,
    role_id,
    name,
    first_name,
    email,
    password,
    degree_id,
    remember_token,
    phone,
    cip
""";

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IDbConnectionFactory connectionFactory, ILogger<UserRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        try
        {
            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

            using var command = connection.CreateCommand();
            command.CommandText = BuildFindUserByUsernameQuery();
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 0;

            AddParameter(command, "username", DbType.String, username, 256);

            using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken)
                .ConfigureAwait(false);

            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                return MapUser(reader);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with username {Username}", username);
            throw;
        }

        return null;
    }

    public async Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

            using var command = connection.CreateCommand();
            command.CommandText = BuildGetUserByIdQuery();
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 0;

            AddParameter(command, "userId", DbType.Int32, userId);

            using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken)
                .ConfigureAwait(false);

            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                return MapUser(reader);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with id {UserId}", userId);
            throw;
        }

        return null;
    }

    public async Task<bool> UpdatePasswordHashAsync(int userId, string passwordHash, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        try
        {
            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

            using var command = connection.CreateCommand();
            command.CommandText = BuildUpdatePasswordQuery();
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 0;

            AddParameter(command, "passwordHash", DbType.String, passwordHash, 255);
            AddParameter(command, "userId", DbType.Int32, userId);

            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password hash for user id {UserId}", userId);
            throw;
        }
    }

    private string BuildFindUserByUsernameQuery()
    {
        var table = GetTableName();
        var usernameParameter = GetParameterReference("username");

        return _connectionFactory.Provider switch
        {
            DatabaseProvider.SqlServer => $"""
SELECT TOP (1)
    {SelectColumns}
FROM {table}
WHERE name = {usernameParameter}
""",
            DatabaseProvider.Oracle => $"""
SELECT
    {SelectColumns}
FROM {table}
WHERE name = {usernameParameter}
FETCH FIRST 1 ROWS ONLY
""",
            _ => $"""
SELECT
    {SelectColumns}
FROM {table}
WHERE name = {usernameParameter}
LIMIT 1
"""
        };
    }

    private string BuildGetUserByIdQuery()
    {
        var table = GetTableName();
        var idParameter = GetParameterReference("userId");

        return _connectionFactory.Provider switch
        {
            DatabaseProvider.SqlServer => $"""
SELECT TOP (1)
    {SelectColumns}
FROM {table}
WHERE id = {idParameter}
""",
            DatabaseProvider.Oracle => $"""
SELECT
    {SelectColumns}
FROM {table}
WHERE id = {idParameter}
FETCH FIRST 1 ROWS ONLY
""",
            _ => $"""
SELECT
    {SelectColumns}
FROM {table}
WHERE id = {idParameter}
LIMIT 1
"""
        };
    }

    private string BuildUpdatePasswordQuery()
    {
        var table = GetTableName();
        var passwordParameter = GetParameterReference("passwordHash");
        var idParameter = GetParameterReference("userId");

        return $"""
UPDATE {table}
SET password = {passwordParameter}
WHERE id = {idParameter}
""";
    }

    private string GetTableName() =>
        _connectionFactory.Provider switch
        {
            DatabaseProvider.SqlServer => "dbo.users",
            DatabaseProvider.PostgreSql => "public.users",
            _ => "users"
        };

    private string GetParameterReference(string name) =>
        _connectionFactory.Provider == DatabaseProvider.Oracle
            ? $":{name}"
            : $"@{name}";

    private DbParameter AddParameter(DbCommand command, string name, DbType dbType, object value, int? size = null)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = GetParameterReference(name);
        parameter.DbType = dbType;

        if (size.HasValue)
        {
            parameter.Size = size.Value;
        }

        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
        return parameter;
    }

    private static User MapUser(DbDataReader reader)
    {
        var idOrdinal = GetOrdinal(reader, "id");
        var roleIdOrdinal = GetOrdinal(reader, "role_id");
        var nameOrdinal = GetOrdinal(reader, "name");
        var firstNameOrdinal = GetOrdinal(reader, "first_name");
        var emailOrdinal = GetOrdinal(reader, "email");
        var passwordOrdinal = GetOrdinal(reader, "password");
        var degreeIdOrdinal = GetOrdinal(reader, "degree_id");
        var rememberTokenOrdinal = GetOrdinal(reader, "remember_token");
        var phoneOrdinal = GetOrdinal(reader, "phone");
        var cipOrdinal = GetOrdinal(reader, "cip");

        return new User
        {
            Id = Convert.ToInt32(reader.GetValue(idOrdinal), CultureInfo.InvariantCulture),
            RoleId = Convert.ToInt32(reader.GetValue(roleIdOrdinal), CultureInfo.InvariantCulture),
            Name = GetRequiredString(reader, nameOrdinal),
            FirstName = GetRequiredString(reader, firstNameOrdinal),
            Email = GetRequiredString(reader, emailOrdinal),
            Password = GetRequiredString(reader, passwordOrdinal),
            DegreeId = GetNullableInt32(reader, degreeIdOrdinal),
            RememberToken = GetNullableString(reader, rememberTokenOrdinal),
            Phone = GetNullableInt64(reader, phoneOrdinal),
            Cip = GetNullableInt64(reader, cipOrdinal)
        };
    }

    private static int GetOrdinal(DbDataReader reader, string columnName)
    {
        try
        {
            return reader.GetOrdinal(columnName);
        }
        catch (IndexOutOfRangeException)
        {
            var upper = columnName.ToUpperInvariant();
            try
            {
                return reader.GetOrdinal(upper);
            }
            catch (IndexOutOfRangeException)
            {
                return reader.GetOrdinal(columnName.ToLowerInvariant());
            }
        }
    }

    private static string GetRequiredString(DbDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal)
            ? string.Empty
            : Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture) ?? string.Empty;

    private static string? GetNullableString(DbDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal)
            ? null
            : Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture);

    private static int? GetNullableInt32(DbDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal))
        {
            return null;
        }

        var value = reader.GetValue(ordinal);
        return value switch
        {
            int intValue => intValue,
            long longValue => unchecked((int)longValue),
            decimal decimalValue => Decimal.ToInt32(decimalValue),
            _ => Convert.ToInt32(value, CultureInfo.InvariantCulture)
        };
    }

    private static long? GetNullableInt64(DbDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal))
        {
            return null;
        }

        var value = reader.GetValue(ordinal);
        return value switch
        {
            long longValue => longValue,
            int intValue => intValue,
            decimal decimalValue => Decimal.ToInt64(decimalValue),
            _ => Convert.ToInt64(value, CultureInfo.InvariantCulture)
        };
    }
}
