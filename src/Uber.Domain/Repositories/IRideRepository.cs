using Uber.Domain.Entities;

namespace Uber.Domain.Repositories;

public interface IRideRepository
{
    Task<IReadOnlyList<Ride>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Ride ride, CancellationToken cancellationToken = default);
    Task<Ride?> GetByIdAsync(int rideId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveRideForCustomerAsync(string customerId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveRideForDriverAsync(string driverId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Ride ride, CancellationToken cancellationToken = default);
}
