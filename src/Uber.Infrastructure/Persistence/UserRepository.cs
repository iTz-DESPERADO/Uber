using Microsoft.EntityFrameworkCore;
using Uber.DAL.DataBase;
using Uber.Domain.Entities;
using Uber.Domain.Repositories;
using Model = Uber.DAL.Entities.User;

namespace Uber.Infrastructure.Persistence;

internal sealed class UserRepository(UberDBContext context) : IUserRepository
{
    private IQueryable<Model> Query => context.Set<Model>();

    public async Task<Customer?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var row = await Query.SingleOrDefaultAsync(row => row.Id == id, cancellationToken);
        return row is null ? null : PersistenceMapper.ToDomain(row);
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await Query.ToListAsync(cancellationToken)).Select(PersistenceMapper.ToDomain).ToList();

    public Task UpdateAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        var row = context.Set<Model>().Local.SingleOrDefault(row => row.Id == entity.Id)
            ?? throw new InvalidOperationException("Load the user in this unit of work before updating it.");
        PersistenceMapper.Apply(entity, row);
        return Task.CompletedTask;
    }
}

