namespace Uber.DAL.Entities;

public class Driver : ApplicationUser
{
    public DateTime? ModifiedAt { get; set; }
    public double Balance { get; set; }
    public string? ImagePath { get; set; }
    public int TotalRatingPoints { get; set; }
    public int TotalRatings { get; set; }
    public double CurrentLng { get; set; }
    public double CurrentLat { get; set; }
    public bool IsActive { get; set; }
    public List<Ride> Rides { get; set; } = [];
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    public string? StripeId { get; set; }
}
