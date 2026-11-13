using DomainEntities = Uber.Domain.Entities;
using Models = Uber.DAL.Entities;

namespace Uber.Infrastructure.Persistence;

internal static class PersistenceMapper
{
    public static DomainEntities.Customer ToDomain(Models.User row) => DomainEntities.Customer.Restore(
        row.Id, row.Name, row.Gender, row.DateOfBirth, row.Email, row.PhoneNumber, row.IsDeleted,
        row.CreatedAt, row.UserName, row.Balance, row.TotalRatingPoints, row.TotalRatings,
        row.StripeId, row.ModifiedAt).Value;

    public static DomainEntities.Driver ToDomain(Models.Driver row)
    {
        var entity = DomainEntities.Driver.Restore(row.Id, row.Name, row.Gender, row.DateOfBirth, row.Email,
            row.PhoneNumber, row.IsDeleted, row.CreatedAt, row.UserName, row.Balance, row.TotalRatingPoints,
            row.TotalRatings, row.StripeId, row.ModifiedAt, row.ImagePath, row.CurrentLat, row.CurrentLng,
            row.IsActive, row.VehicleId).Value;
        entity.Vehicle = row.Vehicle is null ? null : ToDomain(row.Vehicle);
        return entity;
    }

    public static DomainEntities.Admin ToDomain(Models.Admin row) => DomainEntities.Admin.Restore(
        row.Id, row.Name, row.Gender, row.DateOfBirth, row.Email, row.PhoneNumber, row.IsDeleted,
        row.CreatedAt, row.UserName).Value;

    public static DomainEntities.Vehicle ToDomain(Models.Vehicle row) => DomainEntities.Vehicle.Restore(
        row.Id, row.Brand, row.Model, row.YearMade, row.Plate, row.Color, row.SeatingCapacity,
        row.Type, row.Transmission, row.VehicleEngine).Value;

    public static DomainEntities.Ride ToDomain(Models.Ride row)
    {
        var entity = DomainEntities.Ride.Restore(row.Id, row.Status, row.CreatedAt, row.StartLat, row.StartLng,
            row.EndLat, row.EndLng, row.DriverId, row.UserId, row.Duration, row.Distance, row.Price,
            row.PaymentMethod);
        entity.User = row.User is null ? null : ToDomain(row.User);
        entity.Driver = row.Driver is null ? null : ToDomain(row.Driver);
        return entity;
    }

    private static void ApplyProfile(DomainEntities.User entity, Models.ApplicationUser row)
    {
        row.Name = entity.Name; row.Gender = entity.Gender; row.DateOfBirth = entity.DateOfBirth;
        row.Email = entity.Email; row.PhoneNumber = entity.PhoneNumber; row.IsDeleted = entity.IsDeleted;
    }

    public static void Apply(DomainEntities.Customer entity, Models.User row)
    {
        ApplyProfile(entity, row); row.Balance = entity.Balance;
        row.TotalRatingPoints = entity.TotalRatingPoints; row.TotalRatings = entity.TotalRatings;
        row.StripeId = entity.StripeId; row.ModifiedAt = entity.ModifiedAt;
    }

    public static void Apply(DomainEntities.Driver entity, Models.Driver row)
    {
        ApplyProfile(entity, row); row.Balance = entity.Balance;
        row.TotalRatingPoints = entity.TotalRatingPoints; row.TotalRatings = entity.TotalRatings;
        row.StripeId = entity.StripeId; row.ModifiedAt = entity.ModifiedAt;
        row.ImagePath = entity.ImagePath; row.CurrentLat = entity.CurrentLat; row.CurrentLng = entity.CurrentLng;
        row.IsActive = entity.IsActive; row.VehicleId = entity.VehicleId;
    }

    public static void Apply(DomainEntities.Admin entity, Models.Admin row) => ApplyProfile(entity, row);

    public static void Apply(DomainEntities.Vehicle entity, Models.Vehicle row)
    {
        row.Brand = entity.Brand; row.Model = entity.Model; row.YearMade = entity.YearMade;
        row.Plate = entity.Plate; row.Color = entity.Color; row.SeatingCapacity = entity.SeatingCapacity;
        row.Type = entity.Type; row.Transmission = entity.Transmission; row.VehicleEngine = entity.VehicleEngine;
    }

    public static void Apply(DomainEntities.Ride entity, Models.Ride row)
    {
        row.CreatedAt = entity.CreatedAt; row.StartLat = entity.StartLat; row.StartLng = entity.StartLng;
        row.EndLat = entity.EndLat; row.EndLng = entity.EndLng; row.DriverId = entity.DriverId!;
        row.UserId = entity.UserId!; row.Duration = entity.Duration; row.Distance = entity.Distance;
        row.Price = entity.Price; row.PaymentMethod = entity.PaymentMethod; row.Status = entity.Status;
    }
}
