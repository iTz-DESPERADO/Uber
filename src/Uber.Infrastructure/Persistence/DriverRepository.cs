using Microsoft.EntityFrameworkCore;
using Uber.DAL.DataBase;
using Uber.Domain.Entities;
using Uber.Domain.Repositories;
using Model = Uber.DAL.Entities.Driver;

namespace Uber.Infrastructure.Persistence;

internal sealed class DriverRepository(UberDBContext context) : IDriverRepository
{
    private IQueryable<Model> Query => context.Set<Model>().Include(row => row.Vehicle);

    public async Task<Driver?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var row = await Query.SingleOrDefaultAsync(row => row.Id == id, cancellationToken);
        return row is null ? null : PersistenceMapper.ToDomain(row);
    }

    public async Task<IReadOnlyList<Driver>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await Query.ToListAsync(cancellationToken)).Select(PersistenceMapper.ToDomain).ToList();

    public Task UpdateAsync(Driver entity, CancellationToken cancellationToken = default)
    {
        var row = context.Set<Model>().Local.SingleOrDefault(row => row.Id == entity.Id)
            ?? throw new InvalidOperationException("Load the driver in this unit of work before updating it.");
        PersistenceMapper.Apply(entity, row);
        return Task.CompletedTask;
    }
}

