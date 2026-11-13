using Uber.Domain.Entities;

namespace Uber.Domain.Repositories;

public interface IUserRepository
{
    Task<Customer?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Customer entity, CancellationToken cancellationToken = default);
}

