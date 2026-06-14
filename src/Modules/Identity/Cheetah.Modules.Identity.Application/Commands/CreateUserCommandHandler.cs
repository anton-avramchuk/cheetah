using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Modules.Identity.DataAccess.Exceptions;
using Cheetah.Modules.Identity.Domain;
using Cheetah.Modules.Identity.DomainEvents;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Identity.Application.Commands;

public abstract class CreateUserCommandHandler<TUser, TRole>(
    UserManager<TUser> userManager,
    RoleManager<TRole> roleManager,
    IEventBus eventBus)
    : ICommandHandler<CreateUserCommand, Guid>
    where TRole : IdentityRole
    where TUser : IdentityUser<TRole>
{
    protected abstract TUser CreateUser(CreateUserCommand command);

    public async ValueTask<Guid> HandleAsync(CreateUserCommand command, CancellationToken ct = default)
    {
        var user = CreateUser(command);
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

        await eventBus.PublishAsync(new UserCreatedEvent(user.Id, user.UserName, user.Email), ct);

        return user.Id;
    }
}
