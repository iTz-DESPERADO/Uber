using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Entities;
using Uber.Domain.Repositories;

namespace Uber.Application.Rides.RateCustomer;

public sealed class RateCustomerHandler(
    IRideRepository rides,
    IRideRatingRepository ratings,
    IUserRepository customers,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RateCustomerCommand>
{
    public async Task<Result> HandleAsync(
        RateCustomerCommand request,
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

        if (ride.Status != Uber.Domain.Enums.RideStatus.Completed)
        {
            return Result.Failure(RideErrors.RideNotCompleted);
        }

        if (string.IsNullOrWhiteSpace(ride.UserId))
        {
            return Result.Failure(RideErrors.CustomerNotAssigned);
        }

        if (await ratings.ExistsAsync(ride.Id, request.DriverId, cancellationToken))
        {
            return Result.Failure(RideErrors.AlreadyRated);
        }

        var customer = ride.User
            ?? await customers.GetByIdAsync(ride.UserId, cancellationToken);

        if (customer is null)
        {
            return Result.Failure(RideErrors.CustomerNotFound);
        }

        var result = customer.AddRating(request.Rating);

        if (result.IsFailure)
        {
            return result;
        }

        var ratingResult = RideRating.Create(
            ride.Id,
            request.DriverId,
            ride.UserId,
            request.Rating);

        if (ratingResult.IsFailure)
        {
            return Result.Failure(ratingResult.Error!);
        }

        await customers.UpdateAsync(customer, cancellationToken);
        await ratings.AddAsync(ratingResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
