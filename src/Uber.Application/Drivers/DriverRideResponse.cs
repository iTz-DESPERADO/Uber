using Uber.Domain.Enums;

namespace Uber.Application.Drivers;

public sealed record DriverRideResponse(
    int Id,
    string? UserName,
    double? Price,
    DateTime CreatedAt,
    double? Duration,
    double? Distance,
    RideStatus Status);
