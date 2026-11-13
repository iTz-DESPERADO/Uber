using Dapper;
using Uber.Application.Abstractions.Data;
using Uber.Application.Abstractions.Messaging;
using Uber.Application.Rides.GetRideById;
using Uber.Domain.Common;

namespace Uber.Application.Rides.GetActiveRide;

public sealed class GetActiveRideHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetActiveRideQuery, RideDetailsResponse?>
{
    public async Task<Result<RideDetailsResponse?>> HandleAsync(
        GetActiveRideQuery request,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT TOP (1)
                ride.Id,
                ride.CreatedAt,
                ride.StartLat,
                ride.StartLng,
                ride.EndLat,
                ride.EndLng,
                ride.DriverId,
                driver.Name AS DriverName,
                ride.UserId,
                customer.Name AS UserName,
                ride.Status,
                ride.Duration,
                ride.Distance,
                ride.Price,
                ride.PaymentMethod,
                (
                    SELECT TOP (1) rating.Value
                    FROM RideRatings AS rating
                    WHERE rating.RideId = ride.Id
                      AND rating.RaterId = ride.UserId
                ) AS CustomerRatingForDriver,
                (
                    SELECT TOP (1) rating.Value
                    FROM RideRatings AS rating
                    WHERE rating.RideId = ride.Id
                      AND rating.RaterId = ride.DriverId
                ) AS DriverRatingForCustomer
            FROM Rides AS ride
            LEFT JOIN AspNetUsers AS driver ON driver.Id = ride.DriverId
            LEFT JOIN AspNetUsers AS customer ON customer.Id = ride.UserId
            WHERE (ride.DriverId = @ParticipantId OR ride.UserId = @ParticipantId)
              AND (
                    ride.Status IN (1, 4, 5)
                    OR (ride.Status = 0 AND ride.CreatedAt >= DATEADD(MINUTE, -2, SYSUTCDATETIME()))
                  )
            ORDER BY ride.CreatedAt DESC;
            """;

        await using var connection =
            await connectionFactory.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            new { request.ParticipantId },
            cancellationToken: cancellationToken);

        var ride = await connection.QuerySingleOrDefaultAsync<RideDetailsResponse>(command);
        return Result<RideDetailsResponse?>.Success(ride);
    }
}
