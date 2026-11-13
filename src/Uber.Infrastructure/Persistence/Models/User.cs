namespace Uber.DAL.Entities;

public class User : ApplicationUser
{
    public DateTime? ModifiedAt { get; set; }
    public int TotalRatingPoints { get; set; }
    public int TotalRatings { get; set; }
    public string? StripeId { get; set; }
    public double Balance { get; set; }
    public List<Ride> Rides { get; set; } = [];
}
