namespace Uber.Application.Drivers;

public sealed record DriverProfileResponse(
    DriverResponse Driver,
    IReadOnlyList<DriverRideResponse> Rides)
{
    public string Id => Driver.Id;
    public string Name => Driver.Name;
    public string? Email => Driver.Email;
    public string? PhoneNumber => Driver.PhoneNumber;
    public DateTime DateOfBirth => Driver.DateOfBirth;
    public string? ImagePath => Driver.ImagePath;
    public bool IsActive => Driver.IsActive;
    public bool IsDeleted => Driver.IsDeleted;
    public DateTime CreatedAt => Driver.CreatedAt;
    public double Balance => Driver.Balance;
    public Uber.Domain.Enums.Gender Gender => Driver.Gender;

    public DriverVehicleResponse Vehicle => new(
        Driver.VehicleBrand,
        Driver.VehicleModel,
        Driver.VehicleYear,
        Driver.VehiclePlate,
        Driver.VehicleColor,
        Driver.VehicleSeatingCapacity);
}

public sealed record DriverVehicleResponse(
    string? Brand,
    string? Model,
    int? YearMade,
    string? Plate,
    string? Color,
    int? SeatingCapacity);
