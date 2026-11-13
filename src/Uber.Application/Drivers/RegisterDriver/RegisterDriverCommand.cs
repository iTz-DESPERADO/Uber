using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Enums;

namespace Uber.Application.Drivers.RegisterDriver;

public sealed record RegisterDriverCommand(
    string Name,
    string Email,
    string Password,
    string PhoneNumber,
    DateTime DateOfBirth,
    Gender Gender,
    string Brand,
    string Model,
    int YearMade,
    string Plate,
    string Color,
    int SeatingCapacity,
    VehicleType VehicleType,
    Transmission Transmission,
    VehicleEngine VehicleEngine,
    string? ImagePath) : ICommand;
