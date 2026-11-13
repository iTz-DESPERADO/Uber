using Uber.Application.Abstractions.Identity;
using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Entities;
using Uber.Domain.Repositories;

namespace Uber.Application.Drivers.RegisterDriver;

public sealed class RegisterDriverHandler(
    IVehicleRepository vehicles,
    IUnitOfWork unitOfWork,
    IIdentityRegistrationService identity)
    : ICommandHandler<RegisterDriverCommand>
{
    public async Task<Result> HandleAsync(
        RegisterDriverCommand request,
        CancellationToken cancellationToken = default)
    {
        var vehicleResult = Vehicle.Create(
            request.Brand,
            request.Model,
            request.YearMade,
            request.Plate,
            request.Color,
            request.SeatingCapacity,
            request.VehicleType,
            request.Transmission,
            request.VehicleEngine);

        if (vehicleResult.IsFailure)
        {
            return vehicleResult;
        }

        var vehicle = vehicleResult.Value;

        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await vehicles.AddAsync(vehicle, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await identity.RegisterDriverAsync(
                request.Name,
                request.Email,
                request.Password,
                request.PhoneNumber,
                request.DateOfBirth,
                request.Gender,
                vehicle.Id,
                request.ImagePath,
                cancellationToken);
        }, cancellationToken);
    }
}
