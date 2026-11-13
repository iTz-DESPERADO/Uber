using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Uber.Domain.Enums;

namespace Uber.DAL.Entities;

public class Vehicle
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int YearMade { get; set; }
    public string Plate { get; set; } = null!;
    public string Color { get; set; } = null!;
    public int SeatingCapacity { get; set; }
    public VehicleType Type { get; set; }
    public Transmission Transmission { get; set; }
    public VehicleEngine VehicleEngine { get; set; }
}
