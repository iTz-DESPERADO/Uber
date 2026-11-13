using Uber.Domain.Entities;

namespace Uber.Domain.Repositories;

public interface IRideRatingRepository
{
    Task<bool> ExistsAsync(
        int rideId,
        string raterId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        RideRating rating,
        CancellationToken cancellationToken = default);
}
