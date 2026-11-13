using System.ComponentModel.DataAnnotations;
using Uber.Domain.Enums;

namespace Uber.Ui.Models;

public sealed class CreateUserModel
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    [Required] public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

public sealed class CreateDriverModel
{
    public VehicleInputModel? Vehicle { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [Required] public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public IFormFile? File { get; set; }
}

public sealed class VehicleInputModel
{
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int YearMade { get; set; }
    public string Plate { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int SeatingCapacity { get; set; }
    public VehicleType Type { get; set; }
    public Transmission Transmission { get; set; }
    public VehicleEngine VehicleEngine { get; set; }
}

