using Uber.Domain.Common;
using Uber.Domain.Enums;

namespace Uber.Domain.Rides;

public static class RideTransitionRules
{
    public static Result CanMarkDriverWaiting(RideStatus current) =>
        Require(current, RideStatus.Accepted, "Ride.CannotMarkDriverWaiting", "Cannot mark driver as arrived");

    public static Result CanStart(RideStatus current) =>
        Require(current, RideStatus.DriverWaiting, "Ride.CannotStart", "Cannot start ride");

    public static Result CanComplete(RideStatus current) =>
        Require(current, RideStatus.InProgress, "Ride.CannotComplete", "Cannot complete ride");

    private static Result Require(RideStatus current, RideStatus expected, string code, string message) =>
        current == expected
            ? Result.Success()
            : Result.Failure(new Error(code,
                $"{message}. Ride is currently in {current} status. Expected: {expected}"));
}
