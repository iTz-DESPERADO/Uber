using Dapper;
using Uber.Application.Abstractions.Data;
using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Common;

namespace Uber.Application.Drivers.GetAvailability;

public sealed class GetDriverAvailabilityHandler(
    IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetDriverAvailabilityQuery, bool>
{
    public async Task<Result<bool>> HandleAsync(
        GetDriverAvailabilityQuery request,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT IsActive
            FROM AspNetUsers
            WHERE Id = @DriverId
              AND Discriminator = 'Driver';
            """;

        await using var connection =
            await connectionFactory.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            new { request.DriverId },
            cancellationToken: cancellationToken);

        var isActive = await connection.QuerySingleOrDefaultAsync<bool?>(command);

        return isActive.HasValue
            ? Result<bool>.Success(isActive.Value)
            : Result<bool>.Failure(new Error("Driver.NotFound", "Driver not found"));
    }
}
