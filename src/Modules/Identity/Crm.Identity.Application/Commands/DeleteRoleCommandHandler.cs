using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Identity.DataAccess.Exceptions;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Crm.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteRoleCommand>))]
public class DeleteRoleCommandHandler(RoleManager<CrmRole> roleManager)
    : ICommandHandler<DeleteRoleCommand>
{
    public async ValueTask HandleAsync(DeleteRoleCommand command, CancellationToken ct = default)
    {
        var role = await roleManager.FindByIdAsync(command.Id.ToString())
                   ?? throw EntityNotFoundException.For<CrmRole>(command.Id);

        var result = await roleManager.DeleteAsync(role);

        if (!result.Succeeded)
            throw new IdentityException(result);
    }
}
