using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Rides.StartRide;

public sealed record StartRideCommand(int RideId, string DriverId) : ICommand;
