using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Repositories;

namespace Uber.Application.Drivers.AddBalance;

public sealed class AddDriverBalanceHandler(
    IDriverRepository drivers,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddDriverBalanceCommand>
{
    public async Task<Result> HandleAsync(
        AddDriverBalanceCommand request,
        CancellationToken cancellationToken = default)
    {
        var driver = await drivers.GetByIdAsync(request.DriverId, cancellationToken);

        if (driver is null)
        {
            return Result.Failure(new Error("Driver.NotFound", "Driver not found"));
        }

        var result = driver.AddBalance(request.Amount);

        if (result.IsFailure)
        {
            return result;
        }

        await drivers.UpdateAsync(driver, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
