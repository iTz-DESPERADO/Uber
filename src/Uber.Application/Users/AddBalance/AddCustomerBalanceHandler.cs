using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Repositories;

namespace Uber.Application.Users.AddBalance;

public sealed class AddCustomerBalanceHandler(
    IUserRepository customers,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddCustomerBalanceCommand>
{
    public async Task<Result> HandleAsync(
        AddCustomerBalanceCommand request,
        CancellationToken cancellationToken = default)
    {
        var customer = await customers.GetByIdAsync(
            request.CustomerId,
            cancellationToken);

        if (customer is null)
        {
            return Result.Failure(new Error("Customer.NotFound", "User not found"));
        }

        var result = customer.AddBalance(request.Amount);

        if (result.IsFailure)
        {
            return result;
        }

        await customers.UpdateAsync(customer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
