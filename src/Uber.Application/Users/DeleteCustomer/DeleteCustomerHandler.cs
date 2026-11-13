using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Repositories;

namespace Uber.Application.Users.DeleteCustomer;

public sealed class DeleteCustomerHandler(IUserRepository users, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCustomerCommand>
{
    public async Task<Result> HandleAsync(DeleteCustomerCommand request, CancellationToken cancellationToken = default)
    {
        var customer = await users.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null) return Result.Failure(new Error("Customer.NotFound", "User not found"));

        var result = customer.Delete();
        if (result.IsFailure) return result;

        await users.UpdateAsync(customer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
