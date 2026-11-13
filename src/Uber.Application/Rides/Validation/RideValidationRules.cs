using FluentValidation;

namespace Uber.Application.Rides.Validation;

internal static class RideValidationRules
{
    public static IRuleBuilderOptions<T, int> ValidRideId<T>(
        this IRuleBuilder<T, int> rule)
    {
        return rule
            .GreaterThan(0)
            .WithErrorCode("Ride.InvalidId")
            .WithMessage("Ride id must be greater than zero");
    }

    public static IRuleBuilderOptions<T, int> ValidRating<T>(
        this IRuleBuilder<T, int> rule)
    {
        return rule
            .InclusiveBetween(1, 5)
            .WithErrorCode("Rating.Invalid")
            .WithMessage("Rating must be between 1 and 5");
    }
}
