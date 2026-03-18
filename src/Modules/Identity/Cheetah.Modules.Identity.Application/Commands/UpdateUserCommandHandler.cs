using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Modules.Identity.DataAccess.Exceptions;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateUserCommand>))]
public class UpdateUserCommandHandler(UserManager<CrmIdentityUser> userManager, RoleManager<CrmIdentityRole> roleManager)
    : ICommandHandler<UpdateUserCommand>
{
    public async ValueTask HandleAsync(UpdateUserCommand command, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(command.Id.ToString())
                   ?? throw EntityNotFoundException.For<CrmIdentityUser>(command.Id);

        user.ChangeUserName(command.UserName);
        user.ChangeEmail(command.Email);
        user.RefreshSecurityStamp();

        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
            throw new IdentityException(result);

        if (command.RoleIds is not null)
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
            {
                var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                    throw new IdentityException(removeResult);
            }

            if (command.RoleIds.Count > 0)
            {
                var roleNames = await roleManager.Roles
                    .Where(r => command.RoleIds.Contains(r.Id))
                    .Select(r => r.Name!)
                    .ToListAsync(ct);

                var addResult = await userManager.AddToRolesAsync(user, roleNames);
                if (!addResult.Succeeded)
                    throw new IdentityException(addResult);
            }
        }
    }
}
