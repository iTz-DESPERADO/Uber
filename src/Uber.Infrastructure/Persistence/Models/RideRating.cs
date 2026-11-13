namespace Uber.DAL.Entities;

public sealed class RideRating
{
    public int Id { get; set; }
    public int RideId { get; set; }
    public Ride Ride { get; set; } = null!;
    public string RaterId { get; set; } = null!;
    public ApplicationUser Rater { get; set; } = null!;
    public string RatedUserId { get; set; } = null!;
    public ApplicationUser RatedUser { get; set; } = null!;
    public int Value { get; set; }
    public DateTime CreatedAt { get; set; }
}
