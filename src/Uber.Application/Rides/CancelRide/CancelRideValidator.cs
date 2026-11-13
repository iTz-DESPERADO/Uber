using FluentValidation;
using Uber.Application.Rides.Validation;

namespace Uber.Application.Rides.CancelRide;

public sealed class CancelRideValidator : AbstractValidator<CancelRideCommand>
{
    public CancelRideValidator()
    {
        RuleFor(command => command.RideId).ValidRideId();
        RuleFor(command => command.ActorId).NotEmpty();
    }
}
