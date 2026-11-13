using Uber.Domain.Common;
namespace Uber.Domain.Identity;
public sealed class Role
{
    private readonly HashSet<Permission> _permissions = [];
    private Role(string name) => Name = name;
    public string Name { get; }
    public IReadOnlyCollection<Permission> Permissions => _permissions;
    public static Result<Role> Create(string name) => string.IsNullOrWhiteSpace(name)
        ? Result<Role>.Failure(IdentityErrors.RoleNameRequired) : Result<Role>.Success(new(name.Trim()));
    public Result AddPermission(Permission permission) { ArgumentNullException.ThrowIfNull(permission); _permissions.Add(permission); return Result.Success(); }
}
