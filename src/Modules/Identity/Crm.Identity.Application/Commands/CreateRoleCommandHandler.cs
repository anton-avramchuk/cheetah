using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.Application.Commands;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Crm.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateRoleCommand, Guid>))]
public class CreateRoleCommandHandler(RoleManager<CrmIdentityRole> roleManager)
    : Cheetah.Modules.Identity.Application.Commands.CreateRoleCommandHandler<CrmIdentityRole>(roleManager)
{
    protected override CrmIdentityRole CreateRole(CreateRoleCommand command) => CrmIdentityRole.Create(command.Name);
}
