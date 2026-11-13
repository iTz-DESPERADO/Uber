using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Repositories;
using Uber.Domain.Services;

namespace Uber.Application.Rides.CompleteRide;

public sealed class CompleteRideHandler(
    IRideRepository rides,
    IUserRepository customers,
    IDriverRepository drivers,
    IUnitOfWork unitOfWork,
    RidePaymentService paymentService)
    : ICommandHandler<CompleteRideCommand>
{
    public async Task<Result> HandleAsync(
        CompleteRideCommand request,
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

        if (string.IsNullOrWhiteSpace(ride.UserId))
        {
            return Result.Failure(RideErrors.CustomerNotAssigned);
        }

        var customer = ride.User
            ?? await customers.GetByIdAsync(ride.UserId, cancellationToken);
        var driver = ride.Driver
            ?? await drivers.GetByIdAsync(request.DriverId, cancellationToken);

        if (customer is null)
        {
            return Result.Failure(RideErrors.CustomerNotFound);
        }

        if (driver is null)
        {
            return Result.Failure(RideErrors.DriverNotFound);
        }

        var completeResult = ride.Complete();

        if (completeResult.IsFailure)
        {
            return completeResult;
        }

        var paymentResult = paymentService.SettleCompletedRide(ride, customer, driver);

        if (paymentResult.IsFailure)
        {
            return paymentResult;
        }

        await rides.UpdateAsync(ride, cancellationToken);
        await customers.UpdateAsync(customer, cancellationToken);
        await drivers.UpdateAsync(driver, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
