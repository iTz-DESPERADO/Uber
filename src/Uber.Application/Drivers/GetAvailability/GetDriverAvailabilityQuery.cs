using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Drivers.GetAvailability;

public sealed record GetDriverAvailabilityQuery(string DriverId)
    : IQuery<bool>;
