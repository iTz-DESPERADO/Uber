using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Rides.CancelRide;

public sealed record CancelRideCommand(int RideId, string ActorId) : ICommand<string?>;
