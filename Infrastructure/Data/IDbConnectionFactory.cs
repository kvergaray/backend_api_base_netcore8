using System.Data.Common;

namespace backend_api_base_netcore8.Infrastructure.Data;

public interface IDbConnectionFactory
{
    DatabaseProvider Provider { get; }
    Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken);
}
