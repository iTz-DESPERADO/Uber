using Microsoft.EntityFrameworkCore;
using Uber.DAL.DataBase;
using Uber.Domain.Entities;
using Uber.Domain.Repositories;
using Model = Uber.DAL.Entities.Admin;

namespace Uber.Infrastructure.Persistence;

internal sealed class AdminRepository(UberDBContext context) : IAdminRepository
{
    private IQueryable<Model> Query => context.Set<Model>();

    public async Task<Admin?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var row = await Query.SingleOrDefaultAsync(row => row.Id == id, cancellationToken);
        return row is null ? null : PersistenceMapper.ToDomain(row);
    }

    public async Task<IReadOnlyList<Admin>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await Query.ToListAsync(cancellationToken)).Select(PersistenceMapper.ToDomain).ToList();

    public Task UpdateAsync(Admin entity, CancellationToken cancellationToken = default)
    {
        var row = context.Set<Model>().Local.SingleOrDefault(row => row.Id == entity.Id)
            ?? throw new InvalidOperationException("Load the admin in this unit of work before updating it.");
        PersistenceMapper.Apply(entity, row);
        return Task.CompletedTask;
    }
}

