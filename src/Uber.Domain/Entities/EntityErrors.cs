using Uber.Domain.Common;

namespace Uber.Domain.Entities;

public static class EntityErrors
{
    public static readonly Error InvalidPersistenceId = new("Entity.InvalidId", "Entity id must be greater than zero");
    public static readonly Error UserIdRequired = new("Ride.UserRequired", "User id is required");
    public static readonly Error DriverIdRequired = new("Ride.DriverRequired", "Driver id is required");
    public static readonly Error InvalidDistance = new("Ride.InvalidDistance", "Distance must be greater than zero");
    public static readonly Error InvalidDuration = new("Ride.InvalidDuration", "Duration must be greater than zero");
    public static readonly Error InvalidPrice = new("Ride.InvalidPrice", "Price cannot be negative");
    public static readonly Error RidePriceRequired = new("Ride.PriceRequired", "Ride price is required");
    public static readonly Error RideCustomerMismatch = new("Ride.CustomerMismatch", "Customer does not belong to this ride");
    public static readonly Error RideDriverMismatch = new("Ride.DriverMismatch", "Driver does not belong to this ride");
    public static readonly Error RideMustBeCompleted = new("Ride.NotCompleted", "Ride payment can only be settled after completion");
    public static readonly Error InsufficientBalance = new("Customer.InsufficientBalance", "Customer wallet balance is not sufficient for this ride");
    public static readonly Error VehicleBrandRequired = new("Vehicle.BrandRequired", "Vehicle brand is required");
    public static readonly Error VehicleModelRequired = new("Vehicle.ModelRequired", "Vehicle model is required");
    public static readonly Error VehiclePlateRequired = new("Vehicle.PlateRequired", "Vehicle plate is required");
    public static readonly Error VehicleColorRequired = new("Vehicle.ColorRequired", "Vehicle color is required");
    public static readonly Error InvalidVehicleYear = new("Vehicle.InvalidYear", "Vehicle year is invalid");
    public static readonly Error InvalidSeatingCapacity = new("Vehicle.InvalidSeatingCapacity", "Seating capacity must be greater than zero");
    public static readonly Error ProfilePhotoRequired = new("Driver.ProfilePhotoRequired", "Profile photo path is required");
    public static readonly Error InvalidLatitude = new("Driver.InvalidLatitude", "Latitude must be between -90 and 90");
    public static readonly Error InvalidLongitude = new("Driver.InvalidLongitude", "Longitude must be between -180 and 180");
    public static readonly Error RatingRaterRequired = new("Rating.RaterRequired", "Rating author is required");
    public static readonly Error RatedUserRequired = new("Rating.UserRequired", "Rated user is required");
    public static readonly Error CannotRateSelf = new("Rating.CannotRateSelf", "A user cannot rate themselves");
    public static readonly Error InvalidRating = new("Rating.InvalidValue", "Rating must be between 1 and 5");
}
