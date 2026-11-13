using Dapper;
using Uber.Application.Abstractions.Data;
using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Common;

namespace Uber.Application.Admins.GetAdmins;

public sealed class GetAdminsHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetAdminsQuery, IReadOnlyList<AdminResponse>>
{
    public async Task<Result<IReadOnlyList<AdminResponse>>> HandleAsync(
        GetAdminsQuery request,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT Id, Name, DateOfBirth, Email, PhoneNumber, IsDeleted
            FROM AspNetUsers
            WHERE Discriminator = 'Admin'
            ORDER BY CreatedAt DESC;
            """;

        await using var connection =
            await connectionFactory.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        var admins = (await connection.QueryAsync<AdminResponse>(command)).AsList();

        return Result<IReadOnlyList<AdminResponse>>.Success(admins);
    }
}
