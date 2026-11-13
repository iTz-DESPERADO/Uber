using Uber.Domain.Common;

namespace Uber.Domain.Entities;

public sealed class RideRating
{
    private RideRating(
        int id,
        int rideId,
        string raterId,
        string ratedUserId,
        int value,
        DateTime createdAt)
    {
        Id = id;
        RideId = rideId;
        RaterId = raterId;
        RatedUserId = ratedUserId;
        Value = value;
        CreatedAt = createdAt;
    }

    public int Id { get; private set; }
    public int RideId { get; }
    public string RaterId { get; }
    public string RatedUserId { get; }
    public int Value { get; }
    public DateTime CreatedAt { get; }

    public static Result<RideRating> Create(
        int rideId,
        string raterId,
        string ratedUserId,
        int value)
    {
        if (rideId <= 0)
        {
            return Result<RideRating>.Failure(EntityErrors.InvalidPersistenceId);
        }

        if (string.IsNullOrWhiteSpace(raterId))
        {
            return Result<RideRating>.Failure(EntityErrors.RatingRaterRequired);
        }

        if (string.IsNullOrWhiteSpace(ratedUserId))
        {
            return Result<RideRating>.Failure(EntityErrors.RatedUserRequired);
        }

        if (string.Equals(raterId, ratedUserId, StringComparison.Ordinal))
        {
            return Result<RideRating>.Failure(EntityErrors.CannotRateSelf);
        }

        if (value is < 1 or > 5)
        {
            return Result<RideRating>.Failure(EntityErrors.InvalidRating);
        }

        return Result<RideRating>.Success(new RideRating(
            0,
            rideId,
            raterId,
            ratedUserId,
            value,
            DateTime.UtcNow));
    }

    public Result SetPersistenceId(int id)
    {
        if (id <= 0 || Id != 0)
        {
            return Result.Failure(EntityErrors.InvalidPersistenceId);
        }

        Id = id;
        return Result.Success();
    }
}
