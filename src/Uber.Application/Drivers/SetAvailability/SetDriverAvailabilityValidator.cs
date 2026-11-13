using FluentValidation;

namespace Uber.Application.Drivers.SetAvailability;

public sealed class SetDriverAvailabilityValidator
    : AbstractValidator<SetDriverAvailabilityCommand>
{
    public SetDriverAvailabilityValidator()
    {
        RuleFor(command => command.DriverId).NotEmpty();
    }
}
