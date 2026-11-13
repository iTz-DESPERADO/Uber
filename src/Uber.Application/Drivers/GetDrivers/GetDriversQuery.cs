using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Drivers.GetDrivers;

public sealed record GetDriversQuery : IQuery<IReadOnlyList<DriverResponse>>;
