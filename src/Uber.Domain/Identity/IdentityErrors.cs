using Uber.Domain.Common;
namespace Uber.Domain.Identity;
public static class IdentityErrors
{
    public static readonly Error IdRequired = new("User.IdRequired", "User id is required");
    public static readonly Error NameRequired = new("User.NameRequired", "Name is required");
    public static readonly Error InvalidDateOfBirth = new("User.InvalidDateOfBirth", "Date of birth cannot be in the future");
    public static readonly Error InvalidRating = new("User.InvalidRating", "Rating must be between 1 and 5");
    public static readonly Error InvalidBalanceAmount = new("User.InvalidBalanceAmount", "Balance amount must be greater than zero");
    public static readonly Error InvalidBalanceAdjustment = new("User.InvalidBalanceAdjustment", "Balance adjustment must be a finite number");
    public static readonly Error ExternalProviderRequired = new("User.ExternalProviderRequired", "External provider is required");
    public static readonly Error ExternalUserIdRequired = new("User.ExternalUserIdRequired", "External provider user id is required");
    public static readonly Error ExternalIdentityExists = new("User.ExternalIdentityExists", "External identity already exists");
    public static readonly Error RoleNameRequired = new("Role.NameRequired", "Role name is required");
    public static readonly Error PermissionNameRequired = new("Permission.NameRequired", "Permission name is required");
}
