using Cheetah.Core.CQRS;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Modules.Identity.DataAccess.Exceptions;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Cheetah.Modules.Identity.Application.Commands;

public abstract class DeleteRoleCommandHandler<TRole>(RoleManager<TRole> roleManager)
    : ICommandHandler<DeleteRoleCommand>
    where TRole : IdentityRole
{
    public async ValueTask HandleAsync(DeleteRoleCommand command, CancellationToken ct = default)
    {
        var role = await roleManager.FindByIdAsync(command.Id.ToString())
                   ?? throw EntityNotFoundException.For<TRole>(command.Id);

        var result = await roleManager.DeleteAsync(role);

        if (!result.Succeeded)
            throw new IdentityException(result);
    }
}
