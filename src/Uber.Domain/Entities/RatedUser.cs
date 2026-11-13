using Uber.Domain.Common;
using Uber.Domain.Enums;
using Uber.Domain.Identity;
namespace Uber.Domain.Entities;
public abstract class RatedUser : User
{
    protected RatedUser() { }
    protected RatedUser(string id, string name, Gender gender, DateTime dateOfBirth, string? email,
        string? phoneNumber, bool isDeleted, DateTime createdAt, string? userName,
        double balance, int points, int ratings, string? stripeId)
        : base(id, name, gender, dateOfBirth, email, phoneNumber, isDeleted, createdAt, userName)
    { Balance = balance; TotalRatingPoints = points; TotalRatings = ratings; StripeId = stripeId; }
    public double Balance { get; private set; }
    public int TotalRatingPoints { get; private set; }
    public int TotalRatings { get; private set; }
    public string? StripeId { get; private set; }
    public double Rating() => TotalRatings == 0 ? 5 : (double)TotalRatingPoints / TotalRatings;
    public Result AddRating(int rating) { if (rating is < 1 or > 5) return Result.Failure(IdentityErrors.InvalidRating); TotalRatingPoints += rating; TotalRatings++; return Result.Success(); }
    public Result AddBalance(double amount) => AdjustBalance(amount);
    public Result AdjustBalance(double amount)
    {
        if (double.IsNaN(amount) || double.IsInfinity(amount))
            return Result.Failure(IdentityErrors.InvalidBalanceAdjustment);
        Balance += amount;
        return Result.Success();
    }
}
