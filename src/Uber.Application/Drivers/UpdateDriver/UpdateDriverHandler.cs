using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Repositories;

namespace Uber.Application.Drivers.UpdateDriver;

public sealed class UpdateDriverHandler(IDriverRepository drivers, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateDriverCommand>
{
    public async Task<Result> HandleAsync(UpdateDriverCommand request, CancellationToken cancellationToken = default)
    {
        var driver = await drivers.GetByIdAsync(request.DriverId, cancellationToken);
        if (driver is null) return Result.Failure(new Error("Driver.NotFound", "Driver not found"));

        driver.SetGender(request.Gender);

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value) driver.MakeActive(); else driver.MakeInactive();
        }

        var result = driver.Edit(
            request.Name, request.DateOfBirth, driver.ImagePath,
            request.Email, request.PhoneNumber, request.IsDeleted);

        if (result.IsFailure) return result;

        await drivers.UpdateAsync(driver, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
