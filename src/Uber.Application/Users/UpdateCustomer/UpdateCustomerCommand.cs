using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Users.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    string CustomerId,
    string Name,
    DateTime DateOfBirth,
    string? Email,
    string? PhoneNumber,
    bool IsDeleted) : ICommand;
