using Uber.Application.Abstractions.Messaging;
using Uber.Application.Rides.GetRideById;

namespace Uber.Application.Rides.GetActiveRide;

public sealed record GetActiveRideQuery(string ParticipantId)
    : IQuery<RideDetailsResponse?>;
