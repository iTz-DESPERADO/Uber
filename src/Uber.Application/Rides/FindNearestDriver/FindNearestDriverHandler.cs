using Dapper;
using Uber.Application.Abstractions.Data;
using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Common;

namespace Uber.Application.Rides.FindNearestDriver;

public sealed class FindNearestDriverHandler(
    IDbConnectionFactory connectionFactory)
    : IQueryHandler<FindNearestDriverQuery, NearestDriverResponse>
{
    public async Task<Result<NearestDriverResponse>> HandleAsync(
        FindNearestDriverQuery request,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT TOP (1)
                driver.Id AS DriverId,
                CASE
                    WHEN customer.User_TotalRatings = 0 THEN 5.0
                    ELSE CAST(customer.User_TotalRatingPoints AS float) / customer.User_TotalRatings
                END AS UserRating
            FROM AspNetUsers AS driver
            INNER JOIN AspNetUsers AS customer ON customer.Id = @UserId
            WHERE driver.Discriminator = 'Driver'
              AND driver.IsActive = 1
              AND driver.IsDeleted = 0
              AND NOT EXISTS (
                    SELECT 1
                    FROM Rides AS activeRide
                    WHERE activeRide.DriverId = driver.Id
                      AND (
                            activeRide.Status IN (1, 4, 5)
                            OR (activeRide.Status = 0 AND activeRide.CreatedAt >= DATEADD(MINUTE, -2, SYSUTCDATETIME()))
                          )
              )
            ORDER BY
                POWER(driver.CurrentLat - @Latitude, 2) +
                POWER(driver.CurrentLng - @Longitude, 2);
            """;

        await using var connection =
            await connectionFactory.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            new
            {
                request.Latitude,
                request.Longitude,
                request.UserId
            },
            cancellationToken: cancellationToken);

        var driver = await connection
            .QuerySingleOrDefaultAsync<NearestDriverResponse>(command);

        return driver is null
            ? Result<NearestDriverResponse>.Failure(RideErrors.NoDriversAvailable)
            : Result<NearestDriverResponse>.Success(driver);
    }
}
