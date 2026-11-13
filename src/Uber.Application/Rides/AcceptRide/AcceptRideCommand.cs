using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Rides.AcceptRide;

public sealed record AcceptRideCommand(int RideId, string DriverId) : ICommand;
