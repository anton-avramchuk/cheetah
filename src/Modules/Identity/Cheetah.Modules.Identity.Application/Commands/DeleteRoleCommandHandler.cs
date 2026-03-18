using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Modules.Identity.DataAccess.Exceptions;
using Cheetah.Modules.Identity.Domain;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Cheetah.Modules.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteRoleCommand>))]
public class DeleteRoleCommandHandler(RoleManager<CrmIdentityRole> roleManager)
    : ICommandHandler<DeleteRoleCommand>
{
    public async ValueTask HandleAsync(DeleteRoleCommand command, CancellationToken ct = default)
    {
        var role = await roleManager.FindByIdAsync(command.Id.ToString())
                   ?? throw EntityNotFoundException.For<CrmIdentityRole>(command.Id);

        var result = await roleManager.DeleteAsync(role);

        if (!result.Succeeded)
            throw new IdentityException(result);
    }
}
