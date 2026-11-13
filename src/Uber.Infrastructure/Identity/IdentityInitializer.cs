using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Uber.DAL.Entities;

namespace Uber.Infrastructure.Identity;

public static class IdentityInitializer
{
    private static readonly string[] Roles = ["User", "Driver", "Admin"];

    public static async Task InitializeIdentityAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Could not create identity role '{role}': {FormatErrors(result)}");
                }
            }
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var usersWithoutRoles = await userManager.Users.ToListAsync(cancellationToken);

        foreach (var user in usersWithoutRoles)
        {
            if ((await userManager.GetRolesAsync(user)).Count != 0)
            {
                continue;
            }

            var role = user switch
            {
                Driver => "Driver",
                Admin => "Admin",
                User => "User",
                _ => null
            };

            if (role is null)
            {
                continue;
            }

            var result = await userManager.AddToRoleAsync(user, role);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not assign identity role '{role}' to user '{user.Id}': {FormatErrors(result)}");
            }
        }
    }

    private static string FormatErrors(IdentityResult result) =>
        string.Join(" ", result.Errors.Select(error => error.Description));
}
