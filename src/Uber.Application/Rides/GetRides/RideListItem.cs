using Uber.Domain.Enums;

namespace Uber.Application.Rides.GetRides;

public sealed record RideListItem(
    int Id,
    DateTime CreatedAt,
    string? DriverName,
    string? UserName,
    RideStatus Status,
    double? Duration,
    double? Distance,
    double? Price);
