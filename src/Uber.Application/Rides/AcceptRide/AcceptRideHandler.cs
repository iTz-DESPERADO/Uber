using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Repositories;
using Uber.Domain.Services;

namespace Uber.Application.Rides.AcceptRide;

public sealed class AcceptRideHandler(
    IRideRepository rides,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AcceptRideCommand>
{
    public async Task<Result> HandleAsync(
        AcceptRideCommand request,
        CancellationToken cancellationToken = default)
    {
        var ride = await rides.GetByIdAsync(request.RideId, cancellationToken);

        if (ride is null)
        {
            return Result.Failure(RideErrors.NotFound);
        }

        if (!string.Equals(ride.DriverId, request.DriverId, StringComparison.Ordinal))
        {
            return Result.Failure(RideErrors.DriverNotAuthorized);
        }

        var acceptResult = ride.Accept();

        if (acceptResult.IsFailure)
        {
            return acceptResult;
        }

        await rides.UpdateAsync(ride, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
