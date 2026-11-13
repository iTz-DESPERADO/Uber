using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Drivers.SetAvailability;

public sealed record SetDriverAvailabilityCommand(
    string DriverId,
    bool IsActive) : ICommand;
