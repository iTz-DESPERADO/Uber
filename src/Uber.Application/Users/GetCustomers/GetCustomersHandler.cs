using Dapper;
using Uber.Application.Abstractions.Data;
using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Common;

namespace Uber.Application.Users.GetCustomers;

public sealed class GetCustomersHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetCustomersQuery, IReadOnlyList<CustomerResponse>>
{
    public async Task<Result<IReadOnlyList<CustomerResponse>>> HandleAsync(
        GetCustomersQuery request,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT Id, Name, Gender, DateOfBirth, Email, PhoneNumber, IsDeleted,
                   CreatedAt, User_Balance AS Balance,
                   User_TotalRatingPoints AS TotalRatingPoints,
                   User_TotalRatings AS TotalRatings
            FROM AspNetUsers
            WHERE Discriminator = 'User'
            ORDER BY CreatedAt DESC;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        var customers = (await connection.QueryAsync<CustomerResponse>(command)).AsList();
        return Result<IReadOnlyList<CustomerResponse>>.Success(customers);
    }
}
