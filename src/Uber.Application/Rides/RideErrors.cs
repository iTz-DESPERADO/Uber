using Uber.Domain.Common;

namespace Uber.Application.Rides;

public static class RideErrors
{
    public static readonly Error NotFound = new("Ride.NotFound", "Ride not found");

    public static readonly Error CustomerNotAssigned = new(
        "Ride.CustomerNotAssigned",
        "User is not assigned to the ride");

    public static readonly Error DriverNotAssigned = new(
        "Ride.DriverNotAssigned",
        "Driver is not assigned to the ride");

    public static readonly Error CustomerNotFound = new(
        "Ride.CustomerNotFound",
        "User not found");

    public static readonly Error DriverNotFound = new(
        "Ride.DriverNotFound",
        "Driver not found");

    public static readonly Error DriverNotAuthorized = new(
        "Ride.DriverNotAuthorized",
        "Only the driver assigned to this ride can perform this action");

    public static readonly Error CustomerNotAuthorized = new(
        "Ride.CustomerNotAuthorized",
        "Only the customer assigned to this ride can perform this action");

    public static readonly Error ParticipantNotAuthorized = new(
        "Ride.ParticipantNotAuthorized",
        "Only a participant assigned to this ride can perform this action");

    public static readonly Error RideNotCompleted = new(
        "Ride.NotCompleted",
        "This action is only available after the ride is completed");

    public static readonly Error NoDriversAvailable = new(
        "Ride.NoDriversAvailable",
        "No drivers available in your area");

    public static readonly Error CustomerHasActiveRide = new(
        "Ride.CustomerHasActiveRide",
        "Customer already has an active ride");

    public static readonly Error DriverHasActiveRide = new(
        "Ride.DriverHasActiveRide",
        "The selected driver already has an active ride");

    public static readonly Error AlreadyRated = new(
        "Ride.AlreadyRated",
        "You have already submitted a rating for this ride");
}
