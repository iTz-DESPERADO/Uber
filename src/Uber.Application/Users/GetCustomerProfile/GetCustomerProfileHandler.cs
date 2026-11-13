using Dapper;
using Uber.Application.Abstractions.Data;
using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Common;

namespace Uber.Application.Users.GetCustomerProfile;

public sealed class GetCustomerProfileHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetCustomerProfileQuery, CustomerProfileResponse>
{
    public async Task<Result<CustomerProfileResponse>> HandleAsync(
        GetCustomerProfileQuery request,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT Id, Name, Gender, DateOfBirth, Email, PhoneNumber, IsDeleted,
                   CreatedAt, User_Balance AS Balance,
                   User_TotalRatingPoints AS TotalRatingPoints,
                   User_TotalRatings AS TotalRatings
            FROM AspNetUsers
            WHERE Id = @CustomerId AND Discriminator = 'User';

            SELECT ride.Id, driver.Name AS DriverName,
                   driver.ImagePath AS DriverImagePath, ride.Price, ride.CreatedAt,
                   ride.Duration, ride.Distance, ride.Status
            FROM Rides AS ride
            LEFT JOIN AspNetUsers AS driver ON driver.Id = ride.DriverId
            WHERE ride.UserId = @CustomerId
            ORDER BY ride.CreatedAt DESC;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            sql,
            new { request.CustomerId },
            cancellationToken: cancellationToken);

        using var grid = await connection.QueryMultipleAsync(command);
        var customer = await grid.ReadSingleOrDefaultAsync<CustomerResponse>();

        if (customer is null)
        {
            return Result<CustomerProfileResponse>.Failure(
                new Error("Customer.NotFound", "User not found"));
        }

        var rides = (await grid.ReadAsync<CustomerRideResponse>()).AsList();
        return Result<CustomerProfileResponse>.Success(new(customer, rides));
    }
}
