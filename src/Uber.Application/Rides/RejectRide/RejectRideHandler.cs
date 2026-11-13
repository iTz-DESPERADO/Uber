using Dapper;
using Uber.Application.Abstractions.Data;
using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Repositories;

namespace Uber.Application.Rides.RejectRide;

public sealed class RejectRideHandler(
    IRideRepository rides,
    IDbConnectionFactory connectionFactory,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RejectRideCommand, RejectRideResponse>
{
    public async Task<Result<RejectRideResponse>> HandleAsync(
        RejectRideCommand request,
        CancellationToken cancellationToken = default)
    {
        var ride = await rides.GetByIdAsync(request.RideId, cancellationToken);

        if (ride is null)
        {
            return Result<RejectRideResponse>.Failure(RideErrors.NotFound);
        }

        if (!string.Equals(ride.DriverId, request.DriverId, StringComparison.Ordinal))
        {
            return Result<RejectRideResponse>.Failure(RideErrors.DriverNotAuthorized);
        }

        var rejectResult = ride.Reject();

        if (rejectResult.IsFailure)
        {
            return Result<RejectRideResponse>.Failure(rejectResult.Error!);
        }

        await using var connection =
            await connectionFactory.OpenConnectionAsync(cancellationToken);

        const string nearestDriverSql =
            """
            SELECT TOP (1) driver.Id
            FROM AspNetUsers AS driver
            WHERE driver.Discriminator = 'Driver'
              AND driver.IsActive = 1
              AND driver.IsDeleted = 0
              AND driver.Id <> @RejectedDriverId
              AND NOT EXISTS (
                    SELECT 1
                    FROM Rides AS activeRide
                    WHERE activeRide.DriverId = driver.Id
                      AND activeRide.Id <> @RideId
                      AND (
                            activeRide.Status IN (1, 4, 5)
                            OR (activeRide.Status = 0 AND activeRide.CreatedAt >= DATEADD(MINUTE, -2, SYSUTCDATETIME()))
                          )
              )
            ORDER BY
                POWER(driver.CurrentLat - @StartLat, 2) +
                POWER(driver.CurrentLng - @StartLng, 2);
            """;

        var nearestDriverCommand = new CommandDefinition(
            nearestDriverSql,
            new
            {
                RejectedDriverId = ride.DriverId,
                RideId = ride.Id,
                ride.StartLat,
                ride.StartLng
            },
            cancellationToken: cancellationToken);

        var nextDriverId = await connection.QuerySingleOrDefaultAsync<string>(
            nearestDriverCommand);

        if (!string.IsNullOrWhiteSpace(nextDriverId))
        {
            var assignResult = ride.AssignDriver(nextDriverId);

            if (assignResult.IsFailure)
            {
                return Result<RejectRideResponse>.Failure(assignResult.Error!);
            }
        }

        var userRating = await GetUserRatingAsync(
            connection,
            ride.UserId,
            cancellationToken);

        await rides.UpdateAsync(ride, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RejectRideResponse>.Success(new RejectRideResponse(
            nextDriverId,
            ride.StartLat,
            ride.StartLng,
            ride.EndLat,
            ride.EndLng,
            ride.UserId,
            ride.Distance,
            ride.Duration,
            ride.Price,
            userRating));
    }

    private static async Task<double> GetUserRatingAsync(
        System.Data.Common.DbConnection connection,
        string? userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return 5;
        }

        const string sql =
            """
            SELECT CASE
                WHEN User_TotalRatings = 0 THEN 5.0
                ELSE CAST(User_TotalRatingPoints AS float) / User_TotalRatings
            END
            FROM AspNetUsers
            WHERE Id = @UserId;
            """;

        var command = new CommandDefinition(
            sql,
            new { UserId = userId },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<double?>(command) ?? 5;
    }
}
