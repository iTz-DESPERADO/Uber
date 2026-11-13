using System.Security.Claims;
using Uber.Application.Abstractions.Messaging;
using Uber.Application.Users.DeleteCustomer;
using Uber.Application.Users.GetCustomerProfile;
using Uber.Application.Users.GetCustomers;
using Uber.Application.Users.UpdateCustomer;
using Uber.Domain.Identity;
using Uber.Ui.Authorization.Permissions;
using Uber.Ui.Endpoints.Common;

namespace Uber.Ui.Endpoints.Users;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/", GetUsersAsync)
            .RequirePermission(PermissionNames.UsersRead);

        group.MapGet("/me", GetCurrentUserAsync)
            .RequireAuthorization();

        group.MapPut("/me", UpdateCurrentUserAsync)
            .RequireAuthorization();

        group.MapPut("/{userId}", UpdateUserAsync)
            .RequirePermission(PermissionNames.UsersManage);

        group.MapDelete("/{userId}", DeleteUserAsync)
            .RequirePermission(PermissionNames.UsersManage);

        return endpoints;
    }

    private static async Task<IResult> GetUsersAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCustomersQuery(), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> GetCurrentUserAsync(
        ClaimsPrincipal principal, IMediator mediator, CancellationToken cancellationToken)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

        var result = await mediator.Send(new GetCustomerProfileQuery(userId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> UpdateUserAsync(
        string userId, UpdateUserRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateCustomerCommand(
            userId, request.Name, request.DateOfBirth, request.Email,
            request.PhoneNumber, request.IsDeleted), cancellationToken);

        return result.IsSuccess ? Results.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> UpdateCurrentUserAsync(
        UpdateCurrentUserRequest request,
        ClaimsPrincipal principal,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

        var result = await mediator.Send(new UpdateCustomerCommand(
            userId, request.Name, request.DateOfBirth, request.Email,
            request.PhoneNumber, false), cancellationToken);

        return result.IsSuccess ? Results.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> DeleteUserAsync(
        string userId, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteCustomerCommand(userId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToProblem();
    }

    public sealed record UpdateUserRequest(
        string Name, DateTime DateOfBirth, string? Email,
        string? PhoneNumber, bool IsDeleted);

    public sealed record UpdateCurrentUserRequest(
        string Name, DateTime DateOfBirth, string? Email, string? PhoneNumber);
}
