using System.Data.Common;

namespace Uber.Application.Abstractions.Data;

public interface IDbConnectionFactory
{
    Task<DbConnection> OpenConnectionAsync(
        CancellationToken cancellationToken = default);
}
