using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Uber.Domain.Enums;

namespace Uber.DAL.Entities;

public class Ride
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public double StartLat { get; set; }
    public double StartLng { get; set; }
    public double EndLat { get; set; }
    public double EndLng { get; set; }
    public string DriverId { get; set; } = null!;
    public Driver? Driver { get; set; }
    public string UserId { get; set; } = null!;
    public User? User { get; set; }
    public RideStatus Status { get; set; } = RideStatus.Pending;
    public double? Duration { get; set; }
    public double? Distance { get; set; }
    public double? Price { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Wallet;
    public byte[] RowVersion { get; set; } = [];
}
