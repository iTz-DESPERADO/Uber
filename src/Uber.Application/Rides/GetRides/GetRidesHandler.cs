using Dapper;
using Uber.Application.Abstractions.Data;
using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Common;

namespace Uber.Application.Rides.GetRides;

public sealed class GetRidesHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetRidesQuery, IReadOnlyList<RideListItem>>
{
    public async Task<Result<IReadOnlyList<RideListItem>>> HandleAsync(
        GetRidesQuery request,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT
                ride.Id,
                ride.CreatedAt,
                driver.Name AS DriverName,
                customer.Name AS UserName,
                ride.Status,
                ride.Duration,
                ride.Distance,
                ride.Price
            FROM Rides AS ride
            LEFT JOIN AspNetUsers AS driver ON driver.Id = ride.DriverId
            LEFT JOIN AspNetUsers AS customer ON customer.Id = ride.UserId
            ORDER BY ride.CreatedAt DESC;
            """;

        await using var connection =
            await connectionFactory.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            cancellationToken: cancellationToken);

        var rides = (await connection.QueryAsync<RideListItem>(command)).AsList();

        return Result<IReadOnlyList<RideListItem>>.Success(rides);
    }
}
