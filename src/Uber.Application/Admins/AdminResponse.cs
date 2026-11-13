namespace Uber.Application.Admins;

public sealed record AdminResponse(
    string Id,
    string Name,
    DateTime DateOfBirth,
    string? Email,
    string? PhoneNumber,
    bool IsDeleted);
