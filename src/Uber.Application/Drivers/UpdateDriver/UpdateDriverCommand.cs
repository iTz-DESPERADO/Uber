using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Enums;

namespace Uber.Application.Drivers.UpdateDriver;

public sealed record UpdateDriverCommand(
    string DriverId,
    string Name,
    DateTime DateOfBirth,
    string? Email,
    string? PhoneNumber,
    Gender Gender,
    bool IsDeleted,
    bool? IsActive) : ICommand;
