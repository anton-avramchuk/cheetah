using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Seeding;
using Cheetah.Modules.Identity.Domain;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Crm.Identity.DataAccess.Services;

[Export(LifetimeType.Scoped, typeof(IDatabaseSeeder))]
public class IdentityDbContextSeeder(
    RoleManager<CrmIdentityRole> roleManager,
    UserManager<CrmIdentityUser> userManager) : IDatabaseSeeder
{
    private static readonly string[] Roles = ["admin", "recruiter", "client", "lead"];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync();
        await SeedAdminUserAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in Roles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var role = CrmIdentityRole.Create(roleName);
            var result = await roleManager.CreateAsync(role);

            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    private async Task SeedAdminUserAsync()
    {
        const string email = "admin@admin.com";
        const string password = "Admin1234!";

        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var user = CrmIdentityUser.Create(email, email);
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await userManager.AddToRoleAsync(user, "admin");
    }
}
