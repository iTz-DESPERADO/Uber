namespace Uber.Ui.Authorization.Permissions;

public static class PermissionEndpointExtensions
{
    public static RouteHandlerBuilder RequirePermission(
        this RouteHandlerBuilder builder,
        string permission)
    {
        return builder.RequireAuthorization(policy =>
            policy.AddRequirements(new PermissionRequirement(permission)));
    }
}
