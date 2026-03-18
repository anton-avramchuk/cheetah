using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Modules.Identity.DataAccess.Exceptions;
using Cheetah.Modules.Identity.Domain;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Cheetah.Modules.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteUserCommand>))]
public class DeleteUserCommandHandler(UserManager<CrmIdentityUser> userManager)
    : ICommandHandler<DeleteUserCommand>
{
    public async ValueTask HandleAsync(DeleteUserCommand command, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(command.Id.ToString())
                   ?? throw EntityNotFoundException.For<CrmIdentityUser>(command.Id);

        var result = await userManager.DeleteAsync(user);

        if (!result.Succeeded)
            throw new IdentityException(result);
    }
}
