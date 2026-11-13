using FluentValidation;

namespace Uber.Application.Rides.CreateRide;

public sealed class CreateRideValidator : AbstractValidator<CreateRideCommand>
{
    public CreateRideValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.DriverId).NotEmpty();
        RuleFor(command => command.StartLat).InclusiveBetween(-90, 90);
        RuleFor(command => command.EndLat).InclusiveBetween(-90, 90);
        RuleFor(command => command.StartLng).InclusiveBetween(-180, 180);
        RuleFor(command => command.EndLng).InclusiveBetween(-180, 180);
        RuleFor(command => command.Distance).GreaterThan(0);
        RuleFor(command => command.Duration).GreaterThan(0);
        RuleFor(command => command.PaymentMethod).IsInEnum();
    }
}
