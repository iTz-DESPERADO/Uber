namespace Uber.Application.Rides.RejectRide;

public sealed record RejectRideResponse(
    string? NextDriverId,
    double StartLat,
    double StartLng,
    double EndLat,
    double EndLng,
    string? UserId,
    double? Distance,
    double? Duration,
    double? Price,
    double UserRating);
