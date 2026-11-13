using Uber.Application.Abstractions.Identity;
using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Common;

namespace Uber.Application.Users.RegisterCustomer;

public sealed class RegisterCustomerHandler(IIdentityRegistrationService identity)
    : ICommandHandler<RegisterCustomerCommand>
{
    public Task<Result> HandleAsync(
        RegisterCustomerCommand request,
        CancellationToken cancellationToken = default) =>
        identity.RegisterCustomerAsync(
            request.Name,
            request.Email,
            request.Password,
            request.PhoneNumber,
            request.DateOfBirth,
            request.Gender,
            cancellationToken);
}
