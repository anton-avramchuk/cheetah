using Cheetah.Core.CQRS;
using Cheetah.Modules.Identity.Infrastructure.Exceptions;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Cheetah.Modules.Identity.Application.Commands;

public abstract class CreateRoleCommandHandler<TRole>(RoleManager<TRole> roleManager)
    : ICommandHandler<CreateRoleCommand, Guid>
    where TRole : IdentityRole
{
    protected abstract TRole CreateRole(CreateRoleCommand command);

    public async ValueTask<Guid> HandleAsync(CreateRoleCommand command, CancellationToken ct = default)
    {
        var role = CreateRole(command);
        var result = await roleManager.CreateAsync(role);

        if (!result.Succeeded)
            throw new IdentityException(result);

        return role.Id;
    }
}