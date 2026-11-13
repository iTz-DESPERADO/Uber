using Uber.Domain.Entities;

namespace Uber.Domain.Repositories;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Vehicle entity, CancellationToken cancellationToken = default);
    Task AddAsync(Vehicle entity, CancellationToken cancellationToken = default);
}

