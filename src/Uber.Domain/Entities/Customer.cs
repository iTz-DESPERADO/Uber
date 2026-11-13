using Uber.Domain.Common;
using Uber.Domain.Enums;
using Uber.Domain.Identity;
namespace Uber.Domain.Entities;
public sealed class Customer : RatedUser
{
    private Customer() { }
    private Customer(string id, string name, Gender gender, DateTime dateOfBirth, string? email, string? phoneNumber,
        bool isDeleted, DateTime createdAt, string? userName, double balance, int points, int ratings,
        string? stripeId, DateTime? modifiedAt)
        : base(id, name, gender, dateOfBirth, email, phoneNumber, isDeleted, createdAt, userName, balance, points, ratings, stripeId) => ModifiedAt = modifiedAt;
    public override UserType Type => UserType.Customer;
    public DateTime? ModifiedAt { get; private set; }
    public List<Ride> Rides { get; } = [];
    public static Result<Customer> Create(string id, string name, Gender gender, DateTime dateOfBirth,
        string? email, string? phoneNumber, string? userName = null) =>
        Restore(id, name, gender, dateOfBirth, email, phoneNumber, false, DateTime.UtcNow, userName, 0, 0, 0, null, null);
    public static Result<Customer> Restore(string id, string name, Gender gender, DateTime dateOfBirth,
        string? email, string? phoneNumber, bool isDeleted, DateTime createdAt, string? userName,
        double balance, int points, int ratings, string? stripeId, DateTime? modifiedAt)
    { var result = Validate(id, name, dateOfBirth); return result.IsFailure ? Result<Customer>.Failure(result.Error!) :
        Result<Customer>.Success(new(id, name.Trim(), gender, dateOfBirth, email, phoneNumber, isDeleted, createdAt,
            userName, balance, points, ratings, stripeId, modifiedAt)); }
    public Result Edit(string name, DateTime dateOfBirth, string? email, string? phoneNumber, bool isDeleted)
    { var result = EditProfile(name, dateOfBirth, email, phoneNumber, isDeleted); if (result.IsSuccess) ModifiedAt = DateTime.UtcNow; return result; }
}
