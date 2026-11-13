using System.Security.Claims;
using Uber.Application.Abstractions.Messaging;
using Uber.Application.Rides.GetRideById;
using Uber.Application.Rides.GetActiveRide;
using Uber.Application.Rides.GetRides;
using Uber.Domain.Identity;
using Uber.Ui.Authorization.Permissions;
using Uber.Ui.Endpoints.Common;

namespace Uber.Ui.Endpoints.Rides;

public static class RideQueryEndpoints
{
    public static IEndpointRouteBuilder MapRideQueryEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/rides")
            .WithTags("Rides");

        group.MapGet("/", GetRidesAsync)
            .RequirePermission(PermissionNames.RideList);

        group.MapGet("/{rideId:int}", GetRideByIdAsync)
            .RequirePermission(PermissionNames.RideRead);

        group.MapGet("/active", GetActiveRideAsync)
            .RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> GetActiveRideAsync(
        ClaimsPrincipal principal,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var participantId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(participantId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(
            new GetActiveRideQuery(participantId),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        return result.Value is null
            ? Results.NoContent()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetRidesAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetRidesQuery(),
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.ToProblem();
    }

    private static async Task<IResult> GetRideByIdAsync(
        int rideId,
        ClaimsPrincipal principal,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetRideByIdQuery(rideId),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        var currentUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var ride = result.Value;

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Results.Unauthorized();
        }

        if (!principal.IsInRole("Admin")
            && !string.Equals(ride.UserId, currentUserId, StringComparison.Ordinal)
            && !string.Equals(ride.DriverId, currentUserId, StringComparison.Ordinal))
        {
            return Results.Forbid();
        }

        return Results.Ok(ride);
    }
}
