using Uber.Application.Abstractions.Messaging;
using Uber.Application.Rides.Common;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Repositories;

namespace Uber.Application.Rides.DriverArrived;

public sealed class DriverArrivedHandler(
    IRideRepository repository,
    IUnitOfWork unitOfWork)
    : RideTransitionHandler(repository, unitOfWork),
        ICommandHandler<DriverArrivedCommand>
{
    public Task<Result> HandleAsync(
        DriverArrivedCommand request,
        CancellationToken cancellationToken = default) =>
        TransitionAsync(
            request.RideId,
            request.DriverId,
            ride => ride.MarkDriverArrived(),
            cancellationToken);
}
