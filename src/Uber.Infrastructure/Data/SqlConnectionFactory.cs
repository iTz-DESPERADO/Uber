using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Uber.Application.Abstractions.Data;

namespace Uber.Infrastructure.Data;

internal sealed class SqlConnectionFactory(IConfiguration configuration)
    : IDbConnectionFactory
{
    public async Task<DbConnection> OpenConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("Mydatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'Mydatabase' is not configured.");

        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        return connection;
    }
}
