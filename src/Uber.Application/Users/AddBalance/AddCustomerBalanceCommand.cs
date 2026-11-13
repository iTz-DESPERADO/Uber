using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Users.AddBalance;

public sealed record AddCustomerBalanceCommand(
    string CustomerId,
    double Amount) : ICommand;
