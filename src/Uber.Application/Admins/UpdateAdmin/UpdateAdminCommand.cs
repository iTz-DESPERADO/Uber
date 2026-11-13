using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Admins.UpdateAdmin;

public sealed record UpdateAdminCommand(
    string AdminId,
    string Name,
    DateTime DateOfBirth,
    string? Email,
    string? PhoneNumber,
    bool IsDeleted) : ICommand;
