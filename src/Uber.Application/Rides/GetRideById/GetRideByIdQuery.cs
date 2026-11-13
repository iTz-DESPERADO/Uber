using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Rides.GetRideById;

public sealed record GetRideByIdQuery(int RideId)
    : IQuery<RideDetailsResponse>;
