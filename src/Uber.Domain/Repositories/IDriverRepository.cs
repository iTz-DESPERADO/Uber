using Uber.Domain.Entities;

namespace Uber.Domain.Repositories;

public interface IDriverRepository
{
    Task<Driver?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Driver>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Driver entity, CancellationToken cancellationToken = default);
}

