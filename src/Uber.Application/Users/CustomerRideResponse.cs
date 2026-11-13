using Uber.Domain.Enums;

namespace Uber.Application.Users;

public sealed record CustomerRideResponse(
    int Id,
    string? DriverName,
    string? DriverImagePath,
    double? Price,
    DateTime CreatedAt,
    double? Duration,
    double? Distance,
    RideStatus Status);
