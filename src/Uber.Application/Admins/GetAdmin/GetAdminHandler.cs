using Dapper;
using Uber.Application.Abstractions.Data;
using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Common;

namespace Uber.Application.Admins.GetAdmin;

public sealed class GetAdminHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetAdminQuery, AdminResponse>
{
    public async Task<Result<AdminResponse>> HandleAsync(
        GetAdminQuery request,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT Id, Name, DateOfBirth, Email, PhoneNumber, IsDeleted
            FROM AspNetUsers
            WHERE Id = @AdminId AND Discriminator = 'Admin';
            """;

        await using var connection =
            await connectionFactory.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            new { request.AdminId },
            cancellationToken: cancellationToken);

        var admin = await connection.QuerySingleOrDefaultAsync<AdminResponse>(command);

        return admin is null
            ? Result<AdminResponse>.Failure(new Error("Admin.NotFound", "Admin not found"))
            : Result<AdminResponse>.Success(admin);
    }
}
