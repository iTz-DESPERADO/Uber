using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Rides.FindNearestDriver;

public sealed record FindNearestDriverQuery(
    double Latitude,
    double Longitude,
    string UserId) : IQuery<NearestDriverResponse>;
