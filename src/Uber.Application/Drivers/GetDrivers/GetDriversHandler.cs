using Dapper;
using Uber.Application.Abstractions.Data;
using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Common;

namespace Uber.Application.Drivers.GetDrivers;

public sealed class GetDriversHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetDriversQuery, IReadOnlyList<DriverResponse>>
{
    public async Task<Result<IReadOnlyList<DriverResponse>>> HandleAsync(
        GetDriversQuery request, CancellationToken cancellationToken = default)
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
            WHERE driver.Discriminator = 'Driver'
            ORDER BY driver.CreatedAt DESC;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        var drivers = (await connection.QueryAsync<DriverResponse>(command)).AsList();
        return Result<IReadOnlyList<DriverResponse>>.Success(drivers);
    }
}
