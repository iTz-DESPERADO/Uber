using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Uber.Application.Abstractions.Messaging;
using Uber.Application.Rides.CancelRide;
using Uber.Application.Rides.AcceptRide;
using Uber.Application.Rides.CompleteRide;
using Uber.Application.Rides.CreateRide;
using Uber.Application.Rides.DriverArrived;
using Uber.Application.Rides.FindNearestDriver;
using Uber.Application.Rides.RateCustomer;
using Uber.Application.Rides.RateDriver;
using Uber.Application.Rides.RejectRide;
using Uber.Application.Rides.StartRide;
using Uber.Domain.Enums;
using Uber.Domain.Identity;
using Uber.Ui.Authorization.Permissions;
using Uber.Ui.Endpoints.Common;

namespace Uber.Ui.Endpoints.Rides;

public static class RideCommandEndpoints
{
    public static IEndpointRouteBuilder MapRideCommandEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/rides")
            .WithTags("Rides");

        group.MapPost("/", CreateRideAsync)
            .RequirePermission(PermissionNames.RideCreate);

        group.MapPost("/{rideId:int}/cancel", CancelRideAsync)
            .RequirePermission(PermissionNames.RideCancel);

        group.MapPost("/{rideId:int}/driver-arrived", DriverArrivedAsync)
            .RequirePermission(PermissionNames.RideTransition);

        group.MapPost("/{rideId:int}/start", StartRideAsync)
            .RequirePermission(PermissionNames.RideTransition);

        group.MapPost("/{rideId:int}/complete", CompleteRideAsync)
            .RequirePermission(PermissionNames.RideTransition);

        group.MapPost("/{rideId:int}/reject", RejectRideAsync)
            .RequirePermission(PermissionNames.RideReject);

        group.MapPost("/{rideId:int}/accept", AcceptRideAsync)
            .RequirePermission(PermissionNames.RideAccept);

        group.MapPost("/{rideId:int}/rate-customer", RateCustomerAsync)
            .RequirePermission(PermissionNames.RideRateCustomer);

        group.MapPost("/{rideId:int}/rate-driver", RateDriverAsync)
            .RequirePermission(PermissionNames.RideRateDriver);

        return endpoints;
    }

    private static async Task<IResult> CreateRideAsync(
        CreateRideRequest request,
        ClaimsPrincipal principal,
        IMediator mediator,
        IHubContext<RideHub> hub,
        CancellationToken cancellationToken)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }

        var nearestDriverResult = await mediator.Send(
            new FindNearestDriverQuery(request.StartLat, request.StartLng, userId),
            cancellationToken);

        if (nearestDriverResult.IsFailure)
        {
            return nearestDriverResult.ToProblem();
        }

        var nearestDriver = nearestDriverResult.Value;
        var result = await mediator.Send(
            new CreateRideCommand(
                userId,
                nearestDriver.DriverId,
                request.StartLat,
                request.StartLng,
                request.EndLat,
                request.EndLng,
                request.Distance,
                request.Duration,
                request.PaymentMethod),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        var rideId = result.Value.RideId;
        var price = result.Value.Price;
        var rideGroup = $"ride-{rideId}";

        await hub.Clients
            .Group($"driver-{nearestDriver.DriverId}")
            .SendAsync("ReceiveRideRequest", new
            {
                rideId,
                rideGroup,
                request.StartLat,
                request.StartLng,
                request.EndLat,
                request.EndLng,
                userId,
                request.Distance,
                request.Duration,
                price,
                userRating = Math.Round(nearestDriver.UserRating, 1)
            }, cancellationToken);

        return Results.Ok(new
        {
            success = true,
            message = "Ride request created successfully",
            rideId,
            rideGroup
        });
    }

    private static async Task<IResult> AcceptRideAsync(
        int rideId,
        ClaimsPrincipal principal,
        IMediator mediator,
        IHubContext<RideHub> hub,
        CancellationToken cancellationToken)
    {
        var driverId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(driverId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(
            new AcceptRideCommand(rideId, driverId),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        await hub.Clients.Group($"ride-{rideId}")
            .SendAsync("RideAccepted", rideId, cancellationToken);

        return Results.Ok(new { success = true, message = "Ride accepted successfully" });
    }

    private static async Task<IResult> CancelRideAsync(
        int rideId,
        ClaimsPrincipal principal,
        IMediator mediator,
        IHubContext<RideHub> hub,
        CancellationToken cancellationToken)
    {
        var actorId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(actorId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(
            new CancelRideCommand(rideId, actorId), cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        await hub.Clients.Group($"ride-{rideId}")
            .SendAsync("RideCancelled", rideId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(result.Value))
        {
            await hub.Clients.Group($"driver-{result.Value}")
                .SendAsync("RideCancelled", rideId, cancellationToken);
        }

        return Results.Ok(new { success = true, message = "Ride cancelled successfully" });
    }

    private static async Task<IResult> DriverArrivedAsync(
        int rideId, ClaimsPrincipal principal, IMediator mediator,
        IHubContext<RideHub> hub, CancellationToken cancellationToken)
    {
        var driverId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(driverId)) return Results.Unauthorized();

        return await ExecuteTransitionAsync(
            new DriverArrivedCommand(rideId, driverId), "DriverArrived",
            "Driver arrived at pickup location", rideId, mediator, hub, cancellationToken);
    }

    private static async Task<IResult> StartRideAsync(
        int rideId, ClaimsPrincipal principal, IMediator mediator,
        IHubContext<RideHub> hub, CancellationToken cancellationToken)
    {
        var driverId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(driverId)) return Results.Unauthorized();

        return await ExecuteTransitionAsync(
            new StartRideCommand(rideId, driverId), "RideStarted",
            "Ride started successfully", rideId, mediator, hub, cancellationToken);
    }

    private static async Task<IResult> CompleteRideAsync(
        int rideId, ClaimsPrincipal principal, IMediator mediator,
        IHubContext<RideHub> hub, CancellationToken cancellationToken)
    {
        var driverId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(driverId)) return Results.Unauthorized();

        return await ExecuteTransitionAsync(
            new CompleteRideCommand(rideId, driverId), "RideCompleted",
            "Ride completed successfully", rideId, mediator, hub, cancellationToken);
    }

    private static async Task<IResult> RateCustomerAsync(
        int rideId, RatingRequest request, ClaimsPrincipal principal, IMediator mediator,
        IHubContext<RideHub> hub, CancellationToken cancellationToken)
    {
        var driverId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(driverId)) return Results.Unauthorized();

        var result = await mediator.Send(
            new RateCustomerCommand(rideId, request.Rating, driverId), cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        await hub.Clients.Group($"ride-{rideId}")
            .SendAsync("UserRated", rideId, request.Rating, cancellationToken);

        return Results.Ok(new { success = true, message = "User rating added successfully" });
    }

    private static async Task<IResult> RejectRideAsync(
        int rideId,
        ClaimsPrincipal principal,
        IMediator mediator,
        IHubContext<RideHub> hub,
        CancellationToken cancellationToken)
    {
        var driverId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(driverId)) return Results.Unauthorized();

        var result = await mediator.Send(
            new RejectRideCommand(rideId, driverId),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        var response = result.Value;

        if (!string.IsNullOrWhiteSpace(response.NextDriverId))
        {
            await hub.Clients
                .Group($"driver-{response.NextDriverId}")
                .SendAsync("ReceiveRideRequest", new
                {
                    rideId,
                    rideGroup = $"ride-{rideId}",
                    response.StartLat,
                    response.StartLng,
                    response.EndLat,
                    response.EndLng,
                    response.UserId,
                    response.Distance,
                    response.Duration,
                    response.Price,
                    userRating = Math.Round(response.UserRating, 1)
                }, cancellationToken);
        }

        await hub.Clients
            .Group($"ride-{rideId}")
            .SendAsync("RideRejected", rideId, cancellationToken);

        var message = response.NextDriverId is null
            ? "Ride rejected, no more drivers available"
            : "Ride rejected, sent to next driver";

        return Results.Ok(new { success = true, message });
    }

    private static async Task<IResult> RateDriverAsync(
        int rideId, RatingRequest request, ClaimsPrincipal principal, IMediator mediator,
        IHubContext<RideHub> hub, CancellationToken cancellationToken)
    {
        var customerId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(customerId)) return Results.Unauthorized();

        var result = await mediator.Send(
            new RateDriverCommand(rideId, request.Rating, customerId), cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        await hub.Clients.Group($"ride-{rideId}")
            .SendAsync("DriverRated", rideId, request.Rating, cancellationToken);

        return Results.Ok(new { success = true, message = "Driver rating added successfully" });
    }

    private static async Task<IResult> ExecuteTransitionAsync(
        ICommand command,
        string eventName,
        string message,
        int rideId,
        IMediator mediator,
        IHubContext<RideHub> hub,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        await hub.Clients.Group($"ride-{rideId}")
            .SendAsync(eventName, rideId, cancellationToken);

        return Results.Ok(new { success = true, message });
    }

    private sealed record CreateRideRequest(
        double StartLat,
        double StartLng,
        double EndLat,
        double EndLng,
        double Distance,
        double Duration,
        PaymentMethod PaymentMethod);

    private sealed record RatingRequest(int Rating);

}
