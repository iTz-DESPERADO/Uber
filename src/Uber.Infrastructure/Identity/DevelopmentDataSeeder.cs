using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Uber.DAL.DataBase;
using Uber.DAL.Entities;
using Uber.Domain.Enums;

namespace Uber.Infrastructure.Identity;

public static class DevelopmentDataSeeder
{
    private const string AdminPassword = "Admin123!";
    private const string RiderPassword = "Rider123!";
    private const string DriverPassword = "Driver123!";

    public static async Task SeedDevelopmentDataAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = scope.ServiceProvider.GetRequiredService<UberDBContext>();

        await EnsureAccountAsync(userManager, new Admin
        {
            UserName = "admin@uber.local",
            Email = "admin@uber.local",
            EmailConfirmed = true,
            PhoneNumber = "01000000001",
            PhoneNumberConfirmed = true,
            Name = "Uber Administrator",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1990, 1, 1)
        }, AdminPassword, "Admin");

        await EnsureAccountAsync(userManager, new User
        {
            UserName = "rider1@uber.local",
            Email = "rider1@uber.local",
            EmailConfirmed = true,
            PhoneNumber = "01000000002",
            PhoneNumberConfirmed = true,
            Name = "Omar Hassan",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1995, 5, 12),
            Balance = 750
        }, RiderPassword, "User");

        await EnsureAccountAsync(userManager, new User
        {
            UserName = "rider2@uber.local",
            Email = "rider2@uber.local",
            EmailConfirmed = true,
            PhoneNumber = "01100000003",
            PhoneNumberConfirmed = true,
            Name = "Mariam Adel",
            Gender = Gender.Female,
            DateOfBirth = new DateTime(1998, 9, 23),
            Balance = 420
        }, RiderPassword, "User");

        await EnsureDriverAsync(
            userManager,
            context,
            new DriverSeed(
                "driver1@uber.local",
                "Ahmed Samir",
                "01200000004",
                Gender.Male,
                new DateTime(1992, 3, 18),
                30.0444,
                31.2357,
                new Vehicle
                {
                    Brand = "Toyota",
                    Model = "Corolla",
                    YearMade = 2022,
                    Plate = "UBR 1001",
                    Color = "Black",
                    SeatingCapacity = 4,
                    Type = VehicleType.Car,
                    Transmission = Transmission.Automatic,
                    VehicleEngine = VehicleEngine.Gasoline
                }),
            cancellationToken);

        await EnsureDriverAsync(
            userManager,
            context,
            new DriverSeed(
                "driver2@uber.local",
                "Youssef Ali",
                "01500000005",
                Gender.Male,
                new DateTime(1989, 11, 7),
                30.0561,
                31.2394,
                new Vehicle
                {
                    Brand = "Hyundai",
                    Model = "Elantra",
                    YearMade = 2021,
                    Plate = "UBR 1002",
                    Color = "Silver",
                    SeatingCapacity = 4,
                    Type = VehicleType.Car,
                    Transmission = Transmission.Automatic,
                    VehicleEngine = VehicleEngine.Gasoline
                }),
            cancellationToken);
    }

    private static async Task EnsureDriverAsync(
        UserManager<ApplicationUser> userManager,
        UberDBContext context,
        DriverSeed seed,
        CancellationToken cancellationToken)
    {
        if (await userManager.FindByEmailAsync(seed.Email) is not null)
        {
            return;
        }

        var vehicle = await context.Vehicles
            .SingleOrDefaultAsync(item => item.Plate == seed.Vehicle.Plate, cancellationToken);

        if (vehicle is null)
        {
            vehicle = seed.Vehicle;
            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync(cancellationToken);
        }

        await EnsureAccountAsync(userManager, new Driver
        {
            UserName = seed.Email,
            Email = seed.Email,
            EmailConfirmed = true,
            PhoneNumber = seed.PhoneNumber,
            PhoneNumberConfirmed = true,
            Name = seed.Name,
            Gender = seed.Gender,
            DateOfBirth = seed.DateOfBirth,
            Balance = 250,
            IsActive = true,
            CurrentLat = seed.Latitude,
            CurrentLng = seed.Longitude,
            VehicleId = vehicle.Id,
            Vehicle = vehicle
        }, DriverPassword, "Driver");
    }

    private static async Task EnsureAccountAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser account,
        string password,
        string role)
    {
        var existing = await userManager.FindByEmailAsync(account.Email!);

        if (existing is null)
        {
            var createResult = await userManager.CreateAsync(account, password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not seed '{account.Email}': {FormatErrors(createResult)}");
            }

            existing = account;
        }

        if (await userManager.IsInRoleAsync(existing, role))
        {
            return;
        }

        var roleResult = await userManager.AddToRoleAsync(existing, role);

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Could not assign role '{role}' to '{existing.Email}': {FormatErrors(roleResult)}");
        }
    }

    private static string FormatErrors(IdentityResult result) =>
        string.Join(" ", result.Errors.Select(error => error.Description));

    private sealed record DriverSeed(
        string Email,
        string Name,
        string PhoneNumber,
        Gender Gender,
        DateTime DateOfBirth,
        double Latitude,
        double Longitude,
        Vehicle Vehicle);
}
