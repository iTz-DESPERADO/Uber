using Microsoft.EntityFrameworkCore;
using Uber.DAL.DataBase;
using Uber.Domain.Entities;
using Uber.Domain.Repositories;
using Model = Uber.DAL.Entities.Vehicle;

namespace Uber.Infrastructure.Persistence;

internal sealed class VehicleRepository(UberDBContext context) : IVehicleRepository
{
    private IQueryable<Model> Query => context.Set<Model>();

    public async Task<Vehicle?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var row = await Query.SingleOrDefaultAsync(row => row.Id == id, cancellationToken);
        return row is null ? null : PersistenceMapper.ToDomain(row);
    }

    public async Task<IReadOnlyList<Vehicle>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await Query.ToListAsync(cancellationToken)).Select(PersistenceMapper.ToDomain).ToList();

    public Task UpdateAsync(Vehicle entity, CancellationToken cancellationToken = default)
    {
        var row = context.Set<Model>().Local.SingleOrDefault(row => row.Id == entity.Id)
            ?? throw new InvalidOperationException("Load the vehicle in this unit of work before updating it.");
        PersistenceMapper.Apply(entity, row);
        return Task.CompletedTask;
    }

    public async Task AddAsync(Vehicle entity, CancellationToken cancellationToken = default)
    {
        var row = new Model();
        PersistenceMapper.Apply(entity, row);
        await context.Set<Model>().AddAsync(row, cancellationToken);
        context.SynchronizeGeneratedIdAfterSave(() => entity.SetPersistenceId(row.Id));
    }
}

