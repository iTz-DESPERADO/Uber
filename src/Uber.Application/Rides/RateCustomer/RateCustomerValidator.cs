using FluentValidation;
using Uber.Application.Rides.Validation;

namespace Uber.Application.Rides.RateCustomer;

public sealed class RateCustomerValidator : AbstractValidator<RateCustomerCommand>
{
    public RateCustomerValidator()
    {
        RuleFor(command => command.RideId).ValidRideId();
        RuleFor(command => command.Rating).ValidRating();
        RuleFor(command => command.DriverId).NotEmpty();
    }
}
