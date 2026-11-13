using Microsoft.EntityFrameworkCore;
using Uber.DAL.DataBase;
using Uber.Domain.Entities;
using Uber.Domain.Repositories;
using Model = Uber.DAL.Entities.Ride;

namespace Uber.Infrastructure.Persistence;

internal sealed class RideRepository(UberDBContext context) : IRideRepository
{
    private IQueryable<Model> Query => context.Set<Model>().Include(row => row.User).Include(row => row.Driver).ThenInclude(driver => driver!.Vehicle);

    public async Task<Ride?> GetByIdAsync(int rideId, CancellationToken cancellationToken = default)
    {
        var row = await Query.SingleOrDefaultAsync(row => row.Id == rideId, cancellationToken);
        return row is null ? null : PersistenceMapper.ToDomain(row);
    }

    public async Task<IReadOnlyList<Ride>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await Query.ToListAsync(cancellationToken)).Select(PersistenceMapper.ToDomain).ToList();

    public Task<bool> HasActiveRideForCustomerAsync(
        string customerId,
        CancellationToken cancellationToken = default) =>
        HasActiveRideAsync(row => row.UserId == customerId, cancellationToken);

    public Task<bool> HasActiveRideForDriverAsync(
        string driverId,
        CancellationToken cancellationToken = default) =>
        HasActiveRideAsync(row => row.DriverId == driverId, cancellationToken);

    public Task UpdateAsync(Ride entity, CancellationToken cancellationToken = default)
    {
        var row = context.Set<Model>().Local.SingleOrDefault(row => row.Id == entity.Id)
            ?? throw new InvalidOperationException("Load the ride in this unit of work before updating it.");
        PersistenceMapper.Apply(entity, row);
        return Task.CompletedTask;
    }

    public async Task AddAsync(Ride entity, CancellationToken cancellationToken = default)
    {
        var row = new Model();
        PersistenceMapper.Apply(entity, row);
        await context.Set<Model>().AddAsync(row, cancellationToken);
        context.SynchronizeGeneratedIdAfterSave(() => entity.SetPersistenceId(row.Id));
    }

    private Task<bool> HasActiveRideAsync(
        System.Linq.Expressions.Expression<Func<Model, bool>> participantPredicate,
        CancellationToken cancellationToken)
    {
        var pendingCutoff = DateTime.UtcNow.AddMinutes(-2);

        return context.Set<Model>()
            .Where(participantPredicate)
            .AnyAsync(
                row => row.Status == Uber.Domain.Enums.RideStatus.Accepted
                    || row.Status == Uber.Domain.Enums.RideStatus.DriverWaiting
                    || row.Status == Uber.Domain.Enums.RideStatus.InProgress
                    || row.Status == Uber.Domain.Enums.RideStatus.Pending
                        && row.CreatedAt >= pendingCutoff,
                cancellationToken);
    }
}

