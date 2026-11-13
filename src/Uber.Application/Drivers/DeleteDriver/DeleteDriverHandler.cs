using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Repositories;

namespace Uber.Application.Drivers.DeleteDriver;

public sealed class DeleteDriverHandler(IDriverRepository drivers, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteDriverCommand>
{
    public async Task<Result> HandleAsync(DeleteDriverCommand request, CancellationToken cancellationToken = default)
    {
        var driver = await drivers.GetByIdAsync(request.DriverId, cancellationToken);
        if (driver is null) return Result.Failure(new Error("Driver.NotFound", "Driver not found"));

        var result = driver.Delete();
        if (result.IsFailure) return result;

        await drivers.UpdateAsync(driver, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
