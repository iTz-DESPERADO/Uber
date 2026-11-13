using Uber.Domain.Common;
using Uber.Domain.Enums;
using Uber.Domain.Identity;
namespace Uber.Domain.Entities;
public abstract class User
{
    private readonly List<ExternalIdentity> _externalIdentities = [];
    private readonly HashSet<Role> _roles = [];
    protected User() { }
    protected User(string id, string name, Gender gender, DateTime dateOfBirth, string? email,
        string? phoneNumber, bool isDeleted, DateTime createdAt, string? userName)
    { Id = id; Name = name; Gender = gender; DateOfBirth = dateOfBirth; Email = email; PhoneNumber = phoneNumber;
      IsDeleted = isDeleted; CreatedAt = createdAt; UserName = userName; }
    public abstract UserType Type { get; }
    public string Id { get; private set; } = null!;
    public string? UserName { get; private set; }
    public string Name { get; private set; } = null!;
    public Gender Gender { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<ExternalIdentity> ExternalIdentities => _externalIdentities;
    public IReadOnlyCollection<Role> Roles => _roles;
    protected static Result Validate(string id, string name, DateTime dateOfBirth)
    { if (string.IsNullOrWhiteSpace(id)) return Result.Failure(IdentityErrors.IdRequired);
      if (string.IsNullOrWhiteSpace(name)) return Result.Failure(IdentityErrors.NameRequired);
      return dateOfBirth.Date > DateTime.Today ? Result.Failure(IdentityErrors.InvalidDateOfBirth) : Result.Success(); }
    public int Age() { var today = DateTime.Today; var age = today.Year - DateOfBirth.Year; return DateOfBirth.Date > today.AddYears(-age) ? age - 1 : age; }
    public Result Delete() { IsDeleted = true; return Result.Success(); }
    public Result AddExternalIdentity(ExternalIdentity identity)
    { ArgumentNullException.ThrowIfNull(identity);
      if (_externalIdentities.Any(x => string.Equals(x.Provider, identity.Provider, StringComparison.OrdinalIgnoreCase) && x.ProviderUserId == identity.ProviderUserId))
          return Result.Failure(IdentityErrors.ExternalIdentityExists);
      _externalIdentities.Add(identity); return Result.Success(); }
    public Result AssignRole(Role role) { ArgumentNullException.ThrowIfNull(role); _roles.Add(role); return Result.Success(); }
    protected Result EditProfile(string name, DateTime dateOfBirth, string? email, string? phoneNumber, bool isDeleted)
    { var result = Validate(Id, name, dateOfBirth); if (result.IsFailure) return result;
      Name = name.Trim(); DateOfBirth = dateOfBirth; Email = email; PhoneNumber = phoneNumber; IsDeleted = isDeleted; return Result.Success(); }
    protected void ChangeGender(Gender gender) => Gender = gender;
}
