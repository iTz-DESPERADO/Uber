using FluentValidation;
using Uber.Application.Rides.Validation;

namespace Uber.Application.Rides.RateDriver;

public sealed class RateDriverValidator : AbstractValidator<RateDriverCommand>
{
    public RateDriverValidator()
    {
        RuleFor(command => command.RideId).ValidRideId();
        RuleFor(command => command.Rating).ValidRating();
        RuleFor(command => command.CustomerId).NotEmpty();
    }
}
