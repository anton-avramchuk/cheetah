using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Identity.DataAccess.Exceptions;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Cheetah.Modules.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateRoleCommand>))]
public class UpdateRoleCommandHandler(RoleManager<CrmIdentityRole> roleManager)
    : ICommandHandler<UpdateRoleCommand>
{
    public async ValueTask HandleAsync(UpdateRoleCommand command, CancellationToken ct = default)
    {
        var role = await roleManager.FindByIdAsync(command.Id.ToString())
                   ?? throw EntityNotFoundException.For<CrmIdentityRole>(command.Id);

        role.ChangeName(command.Name);
        var result = await roleManager.UpdateAsync(role);

        if (!result.Succeeded)
            throw new IdentityException(result);
    }
}
