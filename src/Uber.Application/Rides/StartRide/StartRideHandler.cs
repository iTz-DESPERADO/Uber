using Uber.Application.Abstractions.Messaging;
using Uber.Application.Rides.Common;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Repositories;

namespace Uber.Application.Rides.StartRide;

public sealed class StartRideHandler(
    IRideRepository repository,
    IUnitOfWork unitOfWork)
    : RideTransitionHandler(repository, unitOfWork),
        ICommandHandler<StartRideCommand>
{
    public Task<Result> HandleAsync(
        StartRideCommand request,
        CancellationToken cancellationToken = default) =>
        TransitionAsync(request.RideId, request.DriverId, ride => ride.Start(), cancellationToken);
}
