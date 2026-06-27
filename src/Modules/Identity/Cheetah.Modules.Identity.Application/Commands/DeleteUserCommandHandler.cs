using Cheetah.Core.CQRS;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Cheetah.Modules.Identity.Infrastructure.Exceptions;
using Cheetah.Modules.Identity.Domain;
using Cheetah.Modules.Identity.DomainEvents;
using Microsoft.AspNetCore.Identity;

namespace Cheetah.Modules.Identity.Application.Commands;

/// <summary>
/// Удаление пользователя. Команда не расширяется хостом (несёт только Id),
/// поэтому хендлер обобщён лишь по доменным типам и регистрируется builder-ом.
/// </summary>
public sealed class DeleteUserCommandHandler<TUser, TRole>(UserManager<TUser> userManager, IEventBus eventBus)
    : ICommandHandler<DeleteUserCommand>
    where TRole : IdentityRole
    where TUser : IdentityUser<TRole>
{
    public async ValueTask HandleAsync(DeleteUserCommand command, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(command.Id.ToString())
                   ?? throw EntityNotFoundException.For<TUser>(command.Id);

        var userName = user.UserName;
        var result = await userManager.DeleteAsync(user);

        if (!result.Succeeded)
            throw new IdentityException(result);

        await eventBus.PublishAsync(new UserDeletedEvent(user.Id, userName), ct);
    }
}
