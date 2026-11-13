using Dapper;
using Uber.Application.Abstractions.Data;
using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Common;

namespace Uber.Application.Drivers.GetDriverProfile;

public sealed class GetDriverProfileHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetDriverProfileQuery, DriverProfileResponse>
{
    public async Task<Result<DriverProfileResponse>> HandleAsync(
        GetDriverProfileQuery request,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT driver.Id, driver.Name, driver.Gender, driver.DateOfBirth,
                   driver.Email, driver.PhoneNumber, driver.IsDeleted, driver.CreatedAt,
                   driver.Balance,
                   driver.ImagePath, driver.IsActive, driver.VehicleId,
                   vehicle.Brand AS VehicleBrand, vehicle.Model AS VehicleModel,
                   vehicle.YearMade AS VehicleYear, vehicle.Plate AS VehiclePlate,
                   vehicle.Color AS VehicleColor,
                   vehicle.SeatingCapacity AS VehicleSeatingCapacity
            FROM AspNetUsers AS driver
            LEFT JOIN Vehicles AS vehicle ON vehicle.Id = driver.VehicleId
            WHERE driver.Id = @DriverId AND driver.Discriminator = 'Driver';

            SELECT ride.Id, customer.Name AS UserName, ride.Price, ride.CreatedAt,
                   ride.Duration, ride.Distance, ride.Status
            FROM Rides AS ride
            LEFT JOIN AspNetUsers AS customer ON customer.Id = ride.UserId
            WHERE ride.DriverId = @DriverId
            ORDER BY ride.CreatedAt DESC;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            sql,
            new { request.DriverId },
            cancellationToken: cancellationToken);

        using var grid = await connection.QueryMultipleAsync(command);
        var driver = await grid.ReadSingleOrDefaultAsync<DriverResponse>();

        if (driver is null)
        {
            return Result<DriverProfileResponse>.Failure(
                new Error("Driver.NotFound", "Driver not found"));
        }

        var rides = (await grid.ReadAsync<DriverRideResponse>()).AsList();
        return Result<DriverProfileResponse>.Success(new(driver, rides));
    }
}
