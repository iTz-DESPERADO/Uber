using Uber.Application.Abstractions.Messaging;
using Uber.Application.Admins.DeleteAdmin;
using Uber.Application.Admins.GetAdmin;
using Uber.Application.Admins.GetAdmins;
using Uber.Application.Admins.UpdateAdmin;
using Uber.Domain.Identity;
using Uber.Ui.Authorization.Permissions;
using Uber.Ui.Endpoints.Common;

namespace Uber.Ui.Endpoints.Admins;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admins").WithTags("Admins");
        group.MapGet("/", GetAdminsAsync).RequirePermission(PermissionNames.AdminsRead);
        group.MapGet("/{adminId}", GetAdminAsync).RequirePermission(PermissionNames.AdminsRead);
        group.MapPut("/{adminId}", UpdateAdminAsync).RequirePermission(PermissionNames.AdminsManage);
        group.MapDelete("/{adminId}", DeleteAdminAsync).RequirePermission(PermissionNames.AdminsManage);
        return endpoints;
    }

    private static async Task<IResult> GetAdminsAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminsQuery(), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> GetAdminAsync(
        string adminId, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminQuery(adminId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> UpdateAdminAsync(
        string adminId, UpdateAdminRequest request,
        IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateAdminCommand(
            adminId, request.Name, request.DateOfBirth, request.Email,
            request.PhoneNumber, request.IsDeleted), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> DeleteAdminAsync(
        string adminId, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteAdminCommand(adminId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToProblem();
    }

    public sealed record UpdateAdminRequest(
        string Name, DateTime DateOfBirth, string? Email,
        string? PhoneNumber, bool IsDeleted);
}
