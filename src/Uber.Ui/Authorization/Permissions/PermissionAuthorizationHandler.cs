using Microsoft.AspNetCore.Authorization;
using Uber.Domain.Identity;

namespace Uber.Ui.Authorization.Permissions;

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var hasPermission = context.User.HasClaim(
            claim => claim.Type == "permission" && claim.Value == requirement.Permission);

        var hasCompatibleRole = context.User.IsInRole("Admin")
            || requirement.Permission == PermissionNames.RideAccept
                && context.User.IsInRole("Driver")
            || requirement.Permission == PermissionNames.RideRead
                && context.User.Identity?.IsAuthenticated == true
            || requirement.Permission == PermissionNames.RideCreate
                && context.User.IsInRole("User")
            || requirement.Permission == PermissionNames.RideCancel
                && context.User.Identity?.IsAuthenticated == true
            || requirement.Permission == PermissionNames.RideRateDriver
                && context.User.IsInRole("User")
            || requirement.Permission is PermissionNames.RideTransition
                or PermissionNames.RideReject
                or PermissionNames.RideRateCustomer
                or PermissionNames.DriverAvailability
                && context.User.IsInRole("Driver")
            || requirement.Permission == PermissionNames.DriverProfile
                && context.User.Identity?.IsAuthenticated == true
            || requirement.Permission == PermissionNames.PaymentsUse
                && context.User.Identity?.IsAuthenticated == true;

        if (hasPermission || hasCompatibleRole)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
