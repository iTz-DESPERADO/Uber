using Uber.Domain.Common;
using Uber.Domain.Enums;

namespace Uber.Domain.Entities;

public sealed class Ride
{
    private Ride() { }
    private Ride(int id, RideStatus status)
    {
        Id = id;
        Status = status;
    }

    public static Ride Restore(int id, RideStatus status, DateTime createdAt, double startLat, double startLng,
        double endLat, double endLng, string? driverId, string? userId, double? duration, double? distance,
        double? price, PaymentMethod paymentMethod) => new(id, status)
    {
        CreatedAt = createdAt, StartLat = startLat, StartLng = startLng, EndLat = endLat, EndLng = endLng,
        DriverId = driverId, UserId = userId, Duration = duration, Distance = distance, Price = price,
        PaymentMethod = paymentMethod
    };

    public static Result<Ride> Create(string userId, string driverId, double startLat, double startLng,
        double endLat, double endLng, double distance, double duration, double price, PaymentMethod paymentMethod)
    {
        if (string.IsNullOrWhiteSpace(userId)) return Result<Ride>.Failure(EntityErrors.UserIdRequired);
        if (string.IsNullOrWhiteSpace(driverId)) return Result<Ride>.Failure(EntityErrors.DriverIdRequired);
        if (distance <= 0) return Result<Ride>.Failure(EntityErrors.InvalidDistance);
        if (duration <= 0) return Result<Ride>.Failure(EntityErrors.InvalidDuration);
        if (price < 0) return Result<Ride>.Failure(EntityErrors.InvalidPrice);
        return Result<Ride>.Success(Restore(0, RideStatus.Pending, DateTime.UtcNow, startLat, startLng,
            endLat, endLng, driverId, userId, duration, distance, price, paymentMethod));
    }

    public int Id { get; private set; }
    public RideStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public double StartLat { get; private set; }
    public double StartLng { get; private set; }
    public double EndLat { get; private set; }
    public double EndLng { get; private set; }
    public string? DriverId { get; private set; }
    public string? UserId { get; private set; }
    public double? Duration { get; private set; }
    public double? Distance { get; private set; }
    public double? Price { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public Customer? User { get; set; }
    public Driver? Driver { get; set; }

    public Result SetPersistenceId(int id)
    {
        if (id <= 0 || Id != 0) return Result.Failure(EntityErrors.InvalidPersistenceId);
        Id = id;
        return Result.Success();
    }

    public Result Accept() => Transition(
        RideStatus.Pending,
        RideStatus.Accepted,
        "Ride.CannotAccept",
        "Only a pending ride can be accepted");

    public Result Reject() => Transition(
        RideStatus.Pending,
        RideStatus.Rejected,
        "Ride.CannotReject",
        "Only a pending ride can be rejected");

    public Result Cancel()
    {
        if (Status is not (RideStatus.Pending or RideStatus.Accepted or RideStatus.DriverWaiting))
        {
            return Result.Failure(new Error(
                "Ride.CannotCancel",
                $"Ride cannot be cancelled while it is in {Status} status"));
        }

        Status = RideStatus.Cancelled;
        return Result.Success();
    }

    public Result AssignDriver(string driverId)
    {
        if (string.IsNullOrWhiteSpace(driverId))
        {
            return Result.Failure(EntityErrors.DriverIdRequired);
        }

        if (Status != RideStatus.Rejected)
        {
            return Result.Failure(new Error(
                "Ride.CannotAssignDriver",
                $"A new driver can only be assigned after rejection. Ride is currently in {Status} status"));
        }

        DriverId = driverId;
        Status = RideStatus.Pending;
        return Result.Success();
    }

    public Result MarkDriverArrived() => Transition(RideStatus.Accepted, RideStatus.DriverWaiting,
        "Ride.CannotMarkDriverWaiting", "Cannot mark driver as arrived");

    public Result Start() => Transition(RideStatus.DriverWaiting, RideStatus.InProgress,
        "Ride.CannotStart", "Cannot start ride");

    public Result Complete() => Transition(RideStatus.InProgress, RideStatus.Completed,
        "Ride.CannotComplete", "Cannot complete ride");

    private Result Transition(RideStatus required, RideStatus target, string code, string message)
    {
        if (Status != required)
            return Result.Failure(new Error(code,
                $"{message}. Ride is currently in {Status} status. Expected: {required}"));

        Status = target;
        return Result.Success();
    }
}
