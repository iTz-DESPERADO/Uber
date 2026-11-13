using FluentValidation;

namespace Uber.Application.Rides.AcceptRide;

public sealed class AcceptRideValidator : AbstractValidator<AcceptRideCommand>
{
    public AcceptRideValidator()
    {
        RuleFor(command => command.RideId)
            .GreaterThan(0)
            .WithErrorCode("Ride.InvalidId")
            .WithMessage("Ride id must be greater than zero");

        RuleFor(command => command.DriverId).NotEmpty();
    }
}
