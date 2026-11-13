using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Entities;
using Uber.Domain.Repositories;

namespace Uber.Application.Rides.Common;

public abstract class RideTransitionHandler(
    IRideRepository repository,
    IUnitOfWork unitOfWork)
{
    protected async Task<Result> TransitionAsync(
        int rideId,
        string driverId,
        Func<Ride, Result> transition,
        CancellationToken cancellationToken)
    {
        var ride = await repository.GetByIdAsync(rideId, cancellationToken);

        if (ride is null)
        {
            return Result.Failure(RideErrors.NotFound);
        }

        if (!string.Equals(ride.DriverId, driverId, StringComparison.Ordinal))
        {
            return Result.Failure(RideErrors.DriverNotAuthorized);
        }

        var result = transition(ride);

        if (result.IsFailure)
        {
            return result;
        }

        await repository.UpdateAsync(ride, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
