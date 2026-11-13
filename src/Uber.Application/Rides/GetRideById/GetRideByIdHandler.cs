using Dapper;
using Uber.Application.Abstractions.Data;
using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Common;

namespace Uber.Application.Rides.GetRideById;

public sealed class GetRideByIdHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetRideByIdQuery, RideDetailsResponse>
{
    public async Task<Result<RideDetailsResponse>> HandleAsync(
        GetRideByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT
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
            WHERE ride.Id = @RideId;
            """;

        await using var connection =
            await connectionFactory.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            new { request.RideId },
            cancellationToken: cancellationToken);

        var ride = await connection.QuerySingleOrDefaultAsync<RideDetailsResponse>(command);

        return ride is null
            ? Result<RideDetailsResponse>.Failure(RideErrors.NotFound)
            : Result<RideDetailsResponse>.Success(ride);
    }
}
