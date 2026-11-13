using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Repositories;

namespace Uber.Application.Users.UpdateCustomer;

public sealed class UpdateCustomerHandler(IUserRepository users, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCustomerCommand>
{
    public async Task<Result> HandleAsync(UpdateCustomerCommand request, CancellationToken cancellationToken = default)
    {
        var customer = await users.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null) return Result.Failure(new Error("Customer.NotFound", "User not found"));

        var result = customer.Edit(request.Name, request.DateOfBirth, request.Email, request.PhoneNumber, request.IsDeleted);
        if (result.IsFailure) return result;

        await users.UpdateAsync(customer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
