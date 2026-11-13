using FluentValidation;

namespace Uber.Application.Rides.GetActiveRide;

public sealed class GetActiveRideValidator : AbstractValidator<GetActiveRideQuery>
{
    public GetActiveRideValidator()
    {
        RuleFor(query => query.ParticipantId).NotEmpty();
    }
}
