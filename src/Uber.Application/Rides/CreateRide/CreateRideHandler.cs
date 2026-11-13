using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Entities;
using Uber.Domain.Repositories;
using Uber.Domain.Services;

namespace Uber.Application.Rides.CreateRide;

public sealed class CreateRideHandler(
    IRideRepository rides,
    IUnitOfWork unitOfWork,
    RidePricingService pricingService)
    : ICommandHandler<CreateRideCommand, CreateRideResponse>
{
    public async Task<Result<CreateRideResponse>> HandleAsync(
        CreateRideCommand request,
        CancellationToken cancellationToken = default)
    {
        if (await rides.HasActiveRideForCustomerAsync(request.UserId, cancellationToken))
        {
            return Result<CreateRideResponse>.Failure(RideErrors.CustomerHasActiveRide);
        }

        if (await rides.HasActiveRideForDriverAsync(request.DriverId, cancellationToken))
        {
            return Result<CreateRideResponse>.Failure(RideErrors.DriverHasActiveRide);
        }

        var priceResult = pricingService.Calculate(request.Distance, request.Duration);

        if (priceResult.IsFailure)
        {
            return Result<CreateRideResponse>.Failure(priceResult.Error!);
        }

        var createResult = Ride.Create(
            request.UserId,
            request.DriverId,
            request.StartLat,
            request.StartLng,
            request.EndLat,
            request.EndLng,
            request.Distance,
            request.Duration,
            priceResult.Value,
            request.PaymentMethod);

        if (createResult.IsFailure)
        {
            return Result<CreateRideResponse>.Failure(createResult.Error!);
        }

        var ride = createResult.Value;

        await rides.AddAsync(ride, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateRideResponse>.Success(new(ride.Id, priceResult.Value));
    }
}
