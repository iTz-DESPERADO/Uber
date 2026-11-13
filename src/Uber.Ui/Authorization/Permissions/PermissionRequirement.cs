using Microsoft.AspNetCore.Authorization;

namespace Uber.Ui.Authorization.Permissions;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;
