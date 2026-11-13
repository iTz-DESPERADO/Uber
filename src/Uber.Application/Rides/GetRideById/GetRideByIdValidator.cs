using FluentValidation;
using Uber.Application.Rides.Validation;

namespace Uber.Application.Rides.GetRideById;

public sealed class GetRideByIdValidator : AbstractValidator<GetRideByIdQuery>
{
    public GetRideByIdValidator()
    {
        RuleFor(query => query.RideId)
            .ValidRideId();
    }
}
