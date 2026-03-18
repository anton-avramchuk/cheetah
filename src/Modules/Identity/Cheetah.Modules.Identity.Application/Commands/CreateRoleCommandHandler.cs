using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.DataAccess.Exceptions;
using Cheetah.Modules.Identity.Domain;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Cheetah.Modules.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateRoleCommand, Guid>))]
public class CreateRoleCommandHandler(RoleManager<CrmIdentityRole> roleManager)
    : ICommandHandler<CreateRoleCommand, Guid>
{
    public async ValueTask<Guid> HandleAsync(CreateRoleCommand command, CancellationToken ct = default)
    {
        var role = CrmIdentityRole.Create(command.Name);
        var result = await roleManager.CreateAsync(role);

        if (!result.Succeeded)
            throw new IdentityException(result);

        return role.Id;
    }
}
