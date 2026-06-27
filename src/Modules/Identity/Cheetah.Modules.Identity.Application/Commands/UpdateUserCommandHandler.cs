using Cheetah.Core.CQRS;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Cheetah.Modules.Identity.Application.Abstractions;
using Cheetah.Modules.Identity.Infrastructure.Exceptions;
using Cheetah.Modules.Identity.Domain;
using Cheetah.Modules.Identity.DomainEvents;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Identity.Application.Commands;

/// <summary>
/// Обобщённый хендлер обновления пользователя. Применение изменений (включая доп. поля)
/// поставляет хост через <see cref="IUpdateUserApplier{TUser,TCommand}"/>.
/// </summary>
public sealed class UpdateUserCommandHandler<TUser, TRole, TCommand>(
    UserManager<TUser> userManager,
    RoleManager<TRole> roleManager,
    IEventBus eventBus,
    IUpdateUserApplier<TUser, TCommand> applier)
    : ICommandHandler<TCommand>
    where TCommand : UpdateUserCommand
    where TRole : IdentityRole
    where TUser : IdentityUser<TRole>
{
    public async ValueTask HandleAsync(TCommand command, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(command.Id.ToString())
                   ?? throw EntityNotFoundException.For<TUser>(command.Id);

        var oldUserName = user.UserName;
        applier.Apply(user, command);

        // Синхронизируем нормализованные UserName/Email через нормализатор UserManager,
        // чтобы поиск (FindByName/FindByEmail) и хранимые значения опирались на один и тот
        // же ILookupNormalizer, даже если он отличается от доменного ToUpperInvariant.
        await userManager.UpdateNormalizedUserNameAsync(user);
        await userManager.UpdateNormalizedEmailAsync(user);

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
                // см. комментарий в CreateUserCommandHandler про RoleManager.Roles
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
