using Uber.Domain.Enums;

namespace Uber.Application.Drivers;

public sealed record DriverResponse(
    string Id,
    string Name,
    Gender Gender,
    DateTime DateOfBirth,
    string? Email,
    string? PhoneNumber,
    bool IsDeleted,
    DateTime CreatedAt,
    double Balance,
    string? ImagePath,
    bool IsActive,
    int VehicleId,
    string? VehicleBrand,
    string? VehicleModel,
    int? VehicleYear,
    string? VehiclePlate,
    string? VehicleColor,
    int? VehicleSeatingCapacity);
