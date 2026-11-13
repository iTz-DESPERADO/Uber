using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Rides.RejectRide;

public sealed record RejectRideCommand(int RideId, string DriverId) : ICommand<RejectRideResponse>;
