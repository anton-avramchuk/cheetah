using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Identity.DataAccess.Exceptions;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Crm.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateRoleCommand, Guid>))]
public class CreateRoleCommandHandler(RoleManager<CrmRole> roleManager)
    : ICommandHandler<CreateRoleCommand, Guid>
{
    public async ValueTask<Guid> HandleAsync(CreateRoleCommand command, CancellationToken ct = default)
    {
        var role = CrmRole.Create(command.Name);
        var result = await roleManager.CreateAsync(role);

        if (!result.Succeeded)
            throw new IdentityException(result);

        return role.Id;
    }
}
