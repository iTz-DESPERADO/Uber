using FluentValidation;
using Uber.Application.Rides.Validation;

namespace Uber.Application.Rides.StartRide;

public sealed class StartRideValidator : AbstractValidator<StartRideCommand>
{
    public StartRideValidator()
    {
        RuleFor(command => command.RideId).ValidRideId();
        RuleFor(command => command.DriverId).NotEmpty();
    }
}
