using Uber.Domain.Common;
namespace Uber.Domain.Identity;
public sealed record Permission
{
    private Permission(string name) => Name = name;
    public string Name { get; }
    public static Result<Permission> Create(string name) => string.IsNullOrWhiteSpace(name)
        ? Result<Permission>.Failure(IdentityErrors.PermissionNameRequired)
        : Result<Permission>.Success(new(name.Trim()));
}
