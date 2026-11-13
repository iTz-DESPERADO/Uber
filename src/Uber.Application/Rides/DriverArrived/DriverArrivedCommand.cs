using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Rides.DriverArrived;

public sealed record DriverArrivedCommand(int RideId, string DriverId) : ICommand;
