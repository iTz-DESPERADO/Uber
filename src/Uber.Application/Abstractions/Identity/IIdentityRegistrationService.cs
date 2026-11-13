using Uber.Domain.Common;
using Uber.Domain.Enums;

namespace Uber.Application.Abstractions.Identity;

public interface IIdentityRegistrationService
{
    Task<Result> RegisterCustomerAsync(
        string name,
        string email,
        string password,
        string phoneNumber,
        DateTime dateOfBirth,
        Gender gender,
        CancellationToken cancellationToken);

    Task<Result> RegisterDriverAsync(
        string name,
        string email,
        string password,
        string phoneNumber,
        DateTime dateOfBirth,
        Gender gender,
        int vehicleId,
        string? imagePath,
        CancellationToken cancellationToken);
}
