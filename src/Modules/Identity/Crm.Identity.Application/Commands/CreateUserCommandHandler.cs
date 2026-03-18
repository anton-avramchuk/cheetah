using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.DataAccess.Exceptions;
using Cheetah.Modules.Identity.Domain;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Crm.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateUserCommand, Guid>))]
public class CreateUserCommandHandler(UserManager<CrmIdentityUser> userManager, RoleManager<CrmIdentityRole> roleManager)
    : ICommandHandler<CreateUserCommand, Guid>
{
    public async ValueTask<Guid> HandleAsync(CreateUserCommand command, CancellationToken ct = default)
    {
        var user = CrmIdentityUser.Create(command.UserName, command.Email);
        var result = await userManager.CreateAsync(user, command.Password);

        if (!result.Succeeded)
            throw new IdentityException(result);

        if (command.RoleIds is { Count: > 0 })
        {
            var roleNames = await roleManager.Roles
                .Where(r => command.RoleIds.Contains(r.Id))
                .Select(r => r.Name!)
                .ToListAsync(ct);

            var roleResult = await userManager.AddToRolesAsync(user, roleNames);
            if (!roleResult.Succeeded)
                throw new IdentityException(roleResult);
        }

        return user.Id;
    }
}
