using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Entities;
using Uber.Domain.Repositories;

namespace Uber.Application.Rides.RateDriver;

public sealed class RateDriverHandler(
    IRideRepository rides,
    IRideRatingRepository ratings,
    IDriverRepository drivers,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RateDriverCommand>
{
    public async Task<Result> HandleAsync(
        RateDriverCommand request,
        CancellationToken cancellationToken = default)
    {
        var ride = await rides.GetByIdAsync(request.RideId, cancellationToken);

        if (ride is null)
        {
            return Result.Failure(RideErrors.NotFound);
        }

        if (!string.Equals(ride.UserId, request.CustomerId, StringComparison.Ordinal))
        {
            return Result.Failure(RideErrors.CustomerNotAuthorized);
        }

        if (ride.Status != Uber.Domain.Enums.RideStatus.Completed)
        {
            return Result.Failure(RideErrors.RideNotCompleted);
        }

        if (string.IsNullOrWhiteSpace(ride.DriverId))
        {
            return Result.Failure(RideErrors.DriverNotAssigned);
        }

        if (await ratings.ExistsAsync(ride.Id, request.CustomerId, cancellationToken))
        {
            return Result.Failure(RideErrors.AlreadyRated);
        }

        var driver = ride.Driver
            ?? await drivers.GetByIdAsync(ride.DriverId, cancellationToken);

        if (driver is null)
        {
            return Result.Failure(RideErrors.DriverNotFound);
        }

        var result = driver.AddRating(request.Rating);

        if (result.IsFailure)
        {
            return result;
        }

        var ratingResult = RideRating.Create(
            ride.Id,
            request.CustomerId,
            ride.DriverId,
            request.Rating);

        if (ratingResult.IsFailure)
        {
            return Result.Failure(ratingResult.Error!);
        }

        await drivers.UpdateAsync(driver, cancellationToken);
        await ratings.AddAsync(ratingResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
