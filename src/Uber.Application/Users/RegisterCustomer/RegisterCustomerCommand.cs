using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Enums;

namespace Uber.Application.Users.RegisterCustomer;

public sealed record RegisterCustomerCommand(
    string Name,
    string Email,
    string Password,
    string PhoneNumber,
    DateTime DateOfBirth,
    Gender Gender) : ICommand;
