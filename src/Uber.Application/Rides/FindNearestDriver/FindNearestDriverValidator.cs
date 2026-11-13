using FluentValidation;

namespace Uber.Application.Rides.FindNearestDriver;

public sealed class FindNearestDriverValidator
    : AbstractValidator<FindNearestDriverQuery>
{
    public FindNearestDriverValidator()
    {
        RuleFor(query => query.Latitude).InclusiveBetween(-90, 90);
        RuleFor(query => query.Longitude).InclusiveBetween(-180, 180);
        RuleFor(query => query.UserId).NotEmpty();
    }
}
