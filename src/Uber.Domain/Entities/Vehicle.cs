using Uber.Domain.Common;
using Uber.Domain.Enums;
namespace Uber.Domain.Entities;
public sealed class Vehicle
{
    private Vehicle() { }
    private Vehicle(int id, string brand, string model, int yearMade, string plate, string color, int seats,
        VehicleType type, Transmission transmission, VehicleEngine engine)
    { Id = id; Brand = brand; Model = model; YearMade = yearMade; Plate = plate; Color = color; SeatingCapacity = seats; Type = type; Transmission = transmission; VehicleEngine = engine; }
    public int Id { get; private set; }
    public string Brand { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public int YearMade { get; private set; }
    public string Plate { get; private set; } = null!;
    public string Color { get; private set; } = null!;
    public int SeatingCapacity { get; private set; }
    public VehicleType Type { get; private set; }
    public Transmission Transmission { get; private set; }
    public VehicleEngine VehicleEngine { get; private set; }
    public static Result<Vehicle> Create(string brand, string model, int yearMade, string plate, string color, int seats,
        VehicleType type, Transmission transmission, VehicleEngine engine) => Restore(0, brand, model, yearMade, plate, color, seats, type, transmission, engine);
    public static Result<Vehicle> Restore(int id, string brand, string model, int yearMade, string plate, string color, int seats,
        VehicleType type, Transmission transmission, VehicleEngine engine)
    {
        if (id < 0) return Result<Vehicle>.Failure(EntityErrors.InvalidPersistenceId);
        if (string.IsNullOrWhiteSpace(brand)) return Result<Vehicle>.Failure(EntityErrors.VehicleBrandRequired);
        if (string.IsNullOrWhiteSpace(model)) return Result<Vehicle>.Failure(EntityErrors.VehicleModelRequired);
        if (string.IsNullOrWhiteSpace(plate)) return Result<Vehicle>.Failure(EntityErrors.VehiclePlateRequired);
        if (string.IsNullOrWhiteSpace(color)) return Result<Vehicle>.Failure(EntityErrors.VehicleColorRequired);
        if (yearMade < 1886 || yearMade > DateTime.UtcNow.Year + 1) return Result<Vehicle>.Failure(EntityErrors.InvalidVehicleYear);
        if (seats <= 0) return Result<Vehicle>.Failure(EntityErrors.InvalidSeatingCapacity);
        return Result<Vehicle>.Success(new(id, brand.Trim(), model.Trim(), yearMade, plate.Trim(), color.Trim(), seats, type, transmission, engine));
    }
    public Result SetPersistenceId(int id) { if (id <= 0 || Id != 0) return Result.Failure(EntityErrors.InvalidPersistenceId); Id = id; return Result.Success(); }
    public string GetTypeName() => Type switch { VehicleType.Car => "Car", VehicleType.Scooter => "Scooter", VehicleType.Shuttle => "Shuttle", _ => string.Empty };
}
