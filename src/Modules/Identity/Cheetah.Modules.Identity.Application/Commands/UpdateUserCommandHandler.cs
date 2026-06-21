using Cheetah.Core.CQRS;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Cheetah.Modules.Identity.DataAccess.Exceptions;
using Cheetah.Modules.Identity.Domain;
using Cheetah.Modules.Identity.DomainEvents;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Identity.Application.Commands;

public abstract class UpdateUserCommandHandler<TUser, TRole>(
    UserManager<TUser> userManager,
    RoleManager<TRole> roleManager,
    IEventBus eventBus)
    : ICommandHandler<UpdateUserCommand>
    where TRole : IdentityRole
    where TUser : IdentityUser<TRole>
{
    protected virtual void ApplyChanges(TUser user, UpdateUserCommand command)
    {
        user.ChangeUserName(command.UserName);
        user.ChangeEmail(command.Email);
        user.RefreshSecurityStamp();
    }

    public async ValueTask HandleAsync(UpdateUserCommand command, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(command.Id.ToString())
                   ?? throw EntityNotFoundException.For<TUser>(command.Id);

        var oldUserName = user.UserName;
        ApplyChanges(user, command);
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
                // Identity намеренно построен на ASP.NET Core Identity (UserManager/RoleManager),
                // а не на Cheetah Repository/Specification. RoleManager.Roles — это штатный API
                // фреймворка, поэтому LINQ-проекция здесь идиоматична и не нарушает правило спецификаций.
                var roleNames = await roleManager.Roles
                    .Where(r => command.RoleIds.Contains(r.Id))
                    .Select(r => r.Name!)
                    .ToListAsync(ct);

                var addResult = await userManager.AddToRolesAsync(user, roleNames);
                if (!addResult.Succeeded)
                    throw new IdentityException(addResult);
            }
        }

        if (!string.Equals(oldUserName, user.UserName, StringComparison.Ordinal))
            await eventBus.PublishAsync(new UserNameChangedEvent(user.Id, oldUserName, user.UserName), ct);
    }
}
