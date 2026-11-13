using Uber.Domain.Common;
using Uber.Domain.Entities;

namespace Uber.Domain.Services;

public sealed class RidePricingService
{
    private const double BaseFare = 7;
    private const double PricePerKilometer = 7;
    private const double PricePerMinute = 1.5;

    public Result<double> Calculate(double distance, double duration)
    {
        if (distance <= 0)
        {
            return Result<double>.Failure(EntityErrors.InvalidDistance);
        }

        if (duration <= 0)
        {
            return Result<double>.Failure(EntityErrors.InvalidDuration);
        }

        var price = BaseFare
            + distance * PricePerKilometer
            + duration * PricePerMinute;

        return Result<double>.Success(Math.Round(price, 2, MidpointRounding.AwayFromZero));
    }
}
