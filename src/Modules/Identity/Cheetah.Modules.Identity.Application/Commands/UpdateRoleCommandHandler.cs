using Cheetah.Core.CQRS;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Modules.Identity.Infrastructure.Exceptions;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Cheetah.Modules.Identity.Application.Commands;

public abstract class UpdateRoleCommandHandler<TRole>(RoleManager<TRole> roleManager)
    : ICommandHandler<UpdateRoleCommand>
    where TRole : IdentityRole
{
    protected virtual void ApplyChanges(TRole role, UpdateRoleCommand command)
    {
        role.ChangeName(command.Name);
    }

    public async ValueTask HandleAsync(UpdateRoleCommand command, CancellationToken ct = default)
    {
        var role = await roleManager.FindByIdAsync(command.Id.ToString())
                   ?? throw EntityNotFoundException.For<TRole>(command.Id);

        ApplyChanges(role, command);
        var result = await roleManager.UpdateAsync(role);

        if (!result.Succeeded)
            throw new IdentityException(result);
    }
}
