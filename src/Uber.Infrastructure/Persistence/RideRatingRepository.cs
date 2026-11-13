using Microsoft.EntityFrameworkCore;
using Uber.DAL.DataBase;
using Uber.Domain.Entities;
using Uber.Domain.Repositories;
using Model = Uber.DAL.Entities.RideRating;

namespace Uber.Infrastructure.Persistence;

internal sealed class RideRatingRepository(UberDBContext context) : IRideRatingRepository
{
    public Task<bool> ExistsAsync(
        int rideId,
        string raterId,
        CancellationToken cancellationToken = default) =>
        context.Set<Model>().AnyAsync(
            rating => rating.RideId == rideId && rating.RaterId == raterId,
            cancellationToken);

    public async Task AddAsync(
        RideRating rating,
        CancellationToken cancellationToken = default)
    {
        var row = new Model
        {
            RideId = rating.RideId,
            RaterId = rating.RaterId,
            RatedUserId = rating.RatedUserId,
            Value = rating.Value,
            CreatedAt = rating.CreatedAt
        };

        await context.Set<Model>().AddAsync(row, cancellationToken);
        context.SynchronizeGeneratedIdAfterSave(() => rating.SetPersistenceId(row.Id));
    }
}
