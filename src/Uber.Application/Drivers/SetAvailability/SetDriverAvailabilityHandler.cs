using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Repositories;

namespace Uber.Application.Drivers.SetAvailability;

public sealed class SetDriverAvailabilityHandler(
    IDriverRepository drivers,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SetDriverAvailabilityCommand>
{
    public async Task<Result> HandleAsync(
        SetDriverAvailabilityCommand request,
        CancellationToken cancellationToken = default)
    {
        var driver = await drivers.GetByIdAsync(
            request.DriverId,
            cancellationToken);

        if (driver is null)
        {
            return Result.Failure(new Error("Driver.NotFound", "Driver not found"));
        }

        var result = request.IsActive
            ? driver.MakeActive()
            : driver.MakeInactive();

        if (result.IsFailure)
        {
            return result;
        }

        await drivers.UpdateAsync(driver, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
