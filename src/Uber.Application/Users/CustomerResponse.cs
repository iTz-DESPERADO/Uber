using Uber.Domain.Enums;

namespace Uber.Application.Users;

public sealed record CustomerResponse(
    string Id,
    string Name,
    Gender Gender,
    DateTime DateOfBirth,
    string? Email,
    string? PhoneNumber,
    bool IsDeleted,
    DateTime CreatedAt,
    double Balance,
    int TotalRatingPoints,
    int TotalRatings)
{
    public int Age => DateTime.UtcNow.Year - DateOfBirth.Year
        - (DateTime.UtcNow.Date < DateOfBirth.Date.AddYears(DateTime.UtcNow.Year - DateOfBirth.Year) ? 1 : 0);

    public double Rating => TotalRatings == 0
        ? 5
        : (double)TotalRatingPoints / TotalRatings;
}
