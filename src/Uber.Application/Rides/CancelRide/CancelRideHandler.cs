using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Repositories;

namespace Uber.Application.Rides.CancelRide;

public sealed class CancelRideHandler(
    IRideRepository rides,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CancelRideCommand, string?>
{
    public async Task<Result<string?>> HandleAsync(
        CancelRideCommand request,
        CancellationToken cancellationToken = default)
    {
        var ride = await rides.GetByIdAsync(request.RideId, cancellationToken);

        if (ride is null)
        {
            return Result<string?>.Failure(RideErrors.NotFound);
        }

        if (!string.Equals(ride.UserId, request.ActorId, StringComparison.Ordinal)
            && !string.Equals(ride.DriverId, request.ActorId, StringComparison.Ordinal))
        {
            return Result<string?>.Failure(RideErrors.ParticipantNotAuthorized);
        }

        var result = ride.Cancel();

        if (result.IsFailure)
        {
            return Result<string?>.Failure(result.Error!);
        }

        await rides.UpdateAsync(ride, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string?>.Success(ride.DriverId);
    }
}
