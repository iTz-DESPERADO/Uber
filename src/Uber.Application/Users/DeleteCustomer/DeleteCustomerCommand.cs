using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Users.DeleteCustomer;

public sealed record DeleteCustomerCommand(string CustomerId) : ICommand;
