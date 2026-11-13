using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Rides.CompleteRide;

public sealed record CompleteRideCommand(int RideId, string DriverId) : ICommand;
