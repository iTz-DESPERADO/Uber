using Uber.Domain.Entities;

namespace Uber.Domain.Repositories;

public interface IAdminRepository
{
    Task<Admin?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Admin>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Admin entity, CancellationToken cancellationToken = default);
}

