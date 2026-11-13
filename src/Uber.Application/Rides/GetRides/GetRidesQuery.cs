using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Rides.GetRides;

public sealed record GetRidesQuery : IQuery<IReadOnlyList<RideListItem>>;
