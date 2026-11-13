using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Enums;

namespace Uber.Application.Rides.CreateRide;

public sealed record CreateRideCommand(
    string UserId,
    string DriverId,
    double StartLat,
    double StartLng,
    double EndLat,
    double EndLng,
    double Distance,
    double Duration,
    PaymentMethod PaymentMethod) : ICommand<CreateRideResponse>;

public sealed record CreateRideResponse(int RideId, double Price);
