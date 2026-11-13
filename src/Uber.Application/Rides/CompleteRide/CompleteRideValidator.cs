using FluentValidation;
using Uber.Application.Rides.Validation;

namespace Uber.Application.Rides.CompleteRide;

public sealed class CompleteRideValidator : AbstractValidator<CompleteRideCommand>
{
    public CompleteRideValidator()
    {
        RuleFor(command => command.RideId).ValidRideId();
        RuleFor(command => command.DriverId).NotEmpty();
    }
}
