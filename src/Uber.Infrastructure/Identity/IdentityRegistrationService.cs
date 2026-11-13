using Microsoft.AspNetCore.Identity;
using Uber.Application.Abstractions.Identity;
using Uber.Domain.Common;
using Uber.Domain.Enums;
using Uber.DAL.Entities;

namespace Uber.Infrastructure.Identity;

public sealed class IdentityRegistrationService(
    UserManager<ApplicationUser> users,
    RoleManager<IdentityRole> roles)
    : IIdentityRegistrationService
{
    public Task<Result> RegisterCustomerAsync(
        string name, string email, string password, string phoneNumber,
        DateTime dateOfBirth, Gender gender, CancellationToken cancellationToken) =>
        CreateAsync(new User
        {
            Name = name,
            Email = email,
            UserName = email,
            PhoneNumber = phoneNumber,
            DateOfBirth = dateOfBirth,
            Gender = gender
        }, password, "User");

    public Task<Result> RegisterDriverAsync(
        string name, string email, string password, string phoneNumber,
        DateTime dateOfBirth, Gender gender, int vehicleId, string? imagePath,
        CancellationToken cancellationToken) =>
        CreateAsync(new Driver
        {
            Name = name,
            Email = email,
            UserName = email,
            PhoneNumber = phoneNumber,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            VehicleId = vehicleId,
            ImagePath = imagePath
        }, password, "Driver");

    private async Task<Result> CreateAsync(
        ApplicationUser user,
        string password,
        string role)
    {
        if (!await roles.RoleExistsAsync(role))
        {
            return Result.Failure(new Error(
                "Identity.RoleNotConfigured",
                $"The required role '{role}' is not configured."));
        }

        var identityResult = await users.CreateAsync(user, password);

        if (!identityResult.Succeeded)
        {
            var message = string.Join(" ", identityResult.Errors.Select(error => error.Description));
            return Result.Failure(new Error("Identity.RegistrationFailed", message));
        }

        var roleResult = await users.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await users.DeleteAsync(user);
            var message = string.Join(" ", roleResult.Errors.Select(error => error.Description));
            return Result.Failure(new Error("Identity.RoleAssignmentFailed", message));
        }

        return Result.Success();
    }
}
