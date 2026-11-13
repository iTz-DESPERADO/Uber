namespace Uber.Application.Rides.FindNearestDriver;

public sealed record NearestDriverResponse(
    string DriverId,
    double UserRating);
