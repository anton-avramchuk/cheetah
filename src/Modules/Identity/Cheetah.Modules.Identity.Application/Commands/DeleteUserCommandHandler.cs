using Cheetah.Core.CQRS;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Modules.Identity.DataAccess.Exceptions;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Cheetah.Modules.Identity.Application.Commands;

public abstract class DeleteUserCommandHandler<TUser, TRole>(UserManager<TUser> userManager)
    : ICommandHandler<DeleteUserCommand>
    where TRole : IdentityRole
    where TUser : IdentityUser<TRole>
{
    public async ValueTask HandleAsync(DeleteUserCommand command, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(command.Id.ToString())
                   ?? throw EntityNotFoundException.For<TUser>(command.Id);

        var result = await userManager.DeleteAsync(user);

        if (!result.Succeeded)
            throw new IdentityException(result);
    }
}
