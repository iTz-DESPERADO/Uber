using FluentValidation;
using Uber.Application.Rides.Validation;

namespace Uber.Application.Rides.RejectRide;

public sealed class RejectRideValidator : AbstractValidator<RejectRideCommand>
{
    public RejectRideValidator()
    {
        RuleFor(command => command.RideId).ValidRideId();
        RuleFor(command => command.DriverId).NotEmpty();
    }
}
