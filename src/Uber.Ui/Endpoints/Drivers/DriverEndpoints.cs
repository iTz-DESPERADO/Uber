using System.Security.Claims;
using Uber.Application.Abstractions.Messaging;
using Uber.Application.Drivers.DeleteDriver;
using Uber.Application.Drivers.GetAvailability;
using Uber.Application.Drivers.GetDriverProfile;
using Uber.Application.Drivers.GetDrivers;
using Uber.Application.Drivers.SetAvailability;
using Uber.Application.Drivers.UpdateDriver;
using Uber.Domain.Enums;
using Uber.Domain.Identity;
using Uber.Ui.Authorization.Permissions;
using Uber.Ui.Endpoints.Common;

namespace Uber.Ui.Endpoints.Drivers;

public static class DriverEndpoints
{
    public static IEndpointRouteBuilder MapDriverEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/drivers").WithTags("Drivers");

        group.MapGet("/", GetDriversAsync)
            .RequirePermission(PermissionNames.DriversRead);
        group.MapGet("/me", GetCurrentDriverAsync)
            .RequirePermission(PermissionNames.DriverProfile);
        group.MapPut("/me", UpdateCurrentDriverAsync)
            .RequirePermission(PermissionNames.DriverProfile);
        group.MapGet("/me/availability", GetAvailabilityAsync)
            .RequirePermission(PermissionNames.DriverAvailability);
        group.MapPut("/me/availability", SetAvailabilityAsync)
            .RequirePermission(PermissionNames.DriverAvailability);
        group.MapPut("/{driverId}", UpdateDriverAsync)
            .RequirePermission(PermissionNames.DriversManage);
        group.MapDelete("/{driverId}", DeleteDriverAsync)
            .RequirePermission(PermissionNames.DriversManage);

        return endpoints;
    }

    private static async Task<IResult> GetDriversAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDriversQuery(), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> GetCurrentDriverAsync(
        ClaimsPrincipal principal, IMediator mediator, CancellationToken cancellationToken)
    {
        var driverId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(driverId)) return Results.Unauthorized();
        var result = await mediator.Send(new GetDriverProfileQuery(driverId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> GetAvailabilityAsync(
        ClaimsPrincipal principal, IMediator mediator, CancellationToken cancellationToken)
    {
        var driverId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(driverId)) return Results.Unauthorized();
        var result = await mediator.Send(new GetDriverAvailabilityQuery(driverId), cancellationToken);
        return result.IsSuccess ? Results.Ok(new { isActive = result.Value }) : result.ToProblem();
    }

    private static async Task<IResult> SetAvailabilityAsync(
        SetAvailabilityRequest request, ClaimsPrincipal principal,
        IMediator mediator, CancellationToken cancellationToken)
    {
        var driverId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(driverId)) return Results.Unauthorized();
        var result = await mediator.Send(
            new SetDriverAvailabilityCommand(driverId, request.IsActive), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> UpdateDriverAsync(
        string driverId, UpdateDriverRequest request,
        IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateDriverCommand(
            driverId, request.Name, request.DateOfBirth, request.Email,
            request.PhoneNumber, request.Gender, request.IsDeleted,
            request.IsActive), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> UpdateCurrentDriverAsync(
        UpdateCurrentDriverRequest request,
        ClaimsPrincipal principal,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var driverId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(driverId)) return Results.Unauthorized();

        var result = await mediator.Send(new UpdateDriverCommand(
            driverId, request.Name, request.DateOfBirth, request.Email,
            request.PhoneNumber, request.Gender, false, null), cancellationToken);

        return result.IsSuccess ? Results.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> DeleteDriverAsync(
        string driverId, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteDriverCommand(driverId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToProblem();
    }

    public sealed record SetAvailabilityRequest(bool IsActive);
    public sealed record UpdateDriverRequest(
        string Name, DateTime DateOfBirth, string? Email,
        string? PhoneNumber, Gender Gender, bool IsDeleted, bool? IsActive);

    public sealed record UpdateCurrentDriverRequest(
        string Name, DateTime DateOfBirth, string? Email,
        string? PhoneNumber, Gender Gender);
}
