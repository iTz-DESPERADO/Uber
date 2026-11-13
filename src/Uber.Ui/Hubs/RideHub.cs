using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Uber.Application.Abstractions.Messaging;
using Uber.Application.Rides.GetRideById;

[Authorize]
public sealed class RideHub(IMediator mediator) : Hub
{
    public async Task JoinRideGroup(int rideId)
    {
        var participantId = Context.UserIdentifier;

        if (string.IsNullOrWhiteSpace(participantId))
        {
            throw new HubException("Authentication is required.");
        }

        var result = await mediator.Send(
            new GetRideByIdQuery(rideId),
            Context.ConnectionAborted);

        if (result.IsFailure)
        {
            throw new HubException("Ride not found.");
        }

        var ride = result.Value;
        var isParticipant = string.Equals(ride.UserId, participantId, StringComparison.Ordinal)
            || string.Equals(ride.DriverId, participantId, StringComparison.Ordinal);

        if (!isParticipant && Context.User?.IsInRole("Admin") != true)
        {
            throw new HubException("You are not a participant in this ride.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"ride-{rideId}",
            Context.ConnectionAborted);
    }

    public Task JoinDriverGroup()
    {
        var driverId = Context.UserIdentifier;

        if (string.IsNullOrWhiteSpace(driverId)
            || Context.User?.IsInRole("Driver") != true)
        {
            throw new HubException("Only authenticated drivers can join a driver group.");
        }

        return Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"driver-{driverId}",
            Context.ConnectionAborted);
    }
}
