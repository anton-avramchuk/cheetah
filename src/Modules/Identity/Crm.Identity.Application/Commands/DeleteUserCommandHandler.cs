using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Identity.DataAccess.Exceptions;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Crm.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteUserCommand>))]
public class DeleteUserCommandHandler(UserManager<CrmUser> userManager)
    : ICommandHandler<DeleteUserCommand>
{
    public async ValueTask HandleAsync(DeleteUserCommand command, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(command.Id.ToString())
                   ?? throw EntityNotFoundException.For<CrmUser>(command.Id);

        var result = await userManager.DeleteAsync(user);

        if (!result.Succeeded)
            throw new IdentityException(result);
    }
}
