using FluentValidation;
using Uber.Application.Rides.Validation;

namespace Uber.Application.Rides.DriverArrived;

public sealed class DriverArrivedValidator : AbstractValidator<DriverArrivedCommand>
{
    public DriverArrivedValidator()
    {
        RuleFor(command => command.RideId).ValidRideId();
        RuleFor(command => command.DriverId).NotEmpty();
    }
}
