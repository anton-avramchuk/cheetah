using Cheetah.Core.CQRS;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Cheetah.Modules.Identity.DataAccess.Exceptions;
using Cheetah.Modules.Identity.Domain;
using Cheetah.Modules.Identity.DomainEvents;
using Microsoft.AspNetCore.Identity;

namespace Cheetah.Modules.Identity.Application.Commands;

public abstract class DeleteUserCommandHandler<TUser, TRole>(UserManager<TUser> userManager, IEventBus eventBus)
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
