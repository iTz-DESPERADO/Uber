using Uber.Domain.Common;
using Uber.Domain.Enums;
using Uber.Domain.Identity;
namespace Uber.Domain.Entities;
public sealed class Admin : User
{
    private Admin() { }
    private Admin(string id, string name, Gender gender, DateTime dateOfBirth, string? email, string? phoneNumber,
        bool isDeleted, DateTime createdAt, string? userName)
        : base(id, name, gender, dateOfBirth, email, phoneNumber, isDeleted, createdAt, userName) { }
    public override UserType Type => UserType.Admin;
    public static Result<Admin> Create(string id, string name, Gender gender, DateTime dateOfBirth,
        string? email, string? phoneNumber, string? userName = null) =>
        Restore(id, name, gender, dateOfBirth, email, phoneNumber, false, DateTime.UtcNow, userName);
    public static Result<Admin> Restore(string id, string name, Gender gender, DateTime dateOfBirth,
        string? email, string? phoneNumber, bool isDeleted, DateTime createdAt, string? userName)
    { var result = Validate(id, name, dateOfBirth); return result.IsFailure ? Result<Admin>.Failure(result.Error!) :
        Result<Admin>.Success(new(id, name.Trim(), gender, dateOfBirth, email, phoneNumber, isDeleted, createdAt, userName)); }
    public Result Edit(string name, DateTime dateOfBirth, string? email, string? phoneNumber, bool isDeleted) =>
        EditProfile(name, dateOfBirth, email, phoneNumber, isDeleted);
}
