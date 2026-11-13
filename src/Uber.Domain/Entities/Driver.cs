using Uber.Domain.Common;
using Uber.Domain.Enums;
using Uber.Domain.Identity;
namespace Uber.Domain.Entities;
public sealed class Driver : RatedUser
{
    private Driver() { }
    private Driver(string id, string name, Gender gender, DateTime dateOfBirth, string? email, string? phoneNumber,
        bool isDeleted, DateTime createdAt, string? userName, double balance, int points, int ratings,
        string? stripeId, DateTime? modifiedAt, string? imagePath, double lat, double lng, bool isActive, int vehicleId)
        : base(id, name, gender, dateOfBirth, email, phoneNumber, isDeleted, createdAt, userName, balance, points, ratings, stripeId)
    { ModifiedAt = modifiedAt; ImagePath = imagePath; CurrentLat = lat; CurrentLng = lng; IsActive = isActive; VehicleId = vehicleId; }
    public override UserType Type => UserType.Driver;
    public DateTime? ModifiedAt { get; private set; }
    public string? ImagePath { get; private set; }
    public double CurrentLat { get; private set; }
    public double CurrentLng { get; private set; }
    public bool IsActive { get; private set; }
    public int VehicleId { get; private set; }
    public Vehicle? Vehicle { get; set; }
    public List<Ride> Rides { get; set; } = [];
    public static Result<Driver> Create(string id, string name, Gender gender, DateTime dateOfBirth,
        string? email, string? phoneNumber, int vehicleId, string? imagePath = null, string? userName = null)
    { if (vehicleId <= 0) return Result<Driver>.Failure(EntityErrors.InvalidPersistenceId);
      return Restore(id, name, gender, dateOfBirth, email, phoneNumber, false, DateTime.UtcNow, userName,
          0, 0, 0, null, null, imagePath, 0, 0, false, vehicleId); }
    public static Result<Driver> Restore(string id, string name, Gender gender, DateTime dateOfBirth,
        string? email, string? phoneNumber, bool isDeleted, DateTime createdAt, string? userName,
        double balance, int points, int ratings, string? stripeId, DateTime? modifiedAt, string? imagePath,
        double lat, double lng, bool isActive, int vehicleId)
    { var result = Validate(id, name, dateOfBirth); if (result.IsFailure) return Result<Driver>.Failure(result.Error!);
      if (vehicleId <= 0) return Result<Driver>.Failure(EntityErrors.InvalidPersistenceId);
      return Result<Driver>.Success(new(id, name.Trim(), gender, dateOfBirth, email, phoneNumber, isDeleted,
          createdAt, userName, balance, points, ratings, stripeId, modifiedAt, imagePath, lat, lng, isActive, vehicleId)); }
    public Result MakeActive() { IsActive = true; return Result.Success(); }
    public Result MakeInactive() { IsActive = false; return Result.Success(); }
    public Result AddProfilePhoto(string path) { if (string.IsNullOrWhiteSpace(path)) return Result.Failure(EntityErrors.ProfilePhotoRequired); if (!IsDeleted) ImagePath = path.Trim(); return Result.Success(); }
    public Result UpdateLocation(double latitude, double longitude)
    { if (latitude is < -90 or > 90) return Result.Failure(EntityErrors.InvalidLatitude);
      if (longitude is < -180 or > 180) return Result.Failure(EntityErrors.InvalidLongitude);
      CurrentLat = latitude; CurrentLng = longitude; return Result.Success(); }
    public Result Edit(string name, DateTime dateOfBirth, string? imagePath, string? email, string? phoneNumber, bool isDeleted)
    { var result = EditProfile(name, dateOfBirth, email, phoneNumber, isDeleted); if (result.IsFailure) return result;
      ImagePath = imagePath; ModifiedAt = DateTime.UtcNow; return Result.Success(); }
    public Result SetGender(Gender gender) { ChangeGender(gender); return Result.Success(); }
}
