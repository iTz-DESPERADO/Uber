using Uber.Domain.Enums;

namespace Uber.Application.Rides.GetRideById;

public sealed record RideDetailsResponse(
    int Id,
    DateTime CreatedAt,
    double StartLat,
    double StartLng,
    double EndLat,
    double EndLng,
    string? DriverId,
    string? DriverName,
    string? UserId,
    string? UserName,
    RideStatus Status,
    double? Duration,
    double? Distance,
    double? Price,
    PaymentMethod PaymentMethod,
    int? CustomerRatingForDriver,
    int? DriverRatingForCustomer);
