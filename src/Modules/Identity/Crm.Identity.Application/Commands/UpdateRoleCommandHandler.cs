using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.Application.Commands;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Crm.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateRoleCommand>))]
public class UpdateRoleCommandHandler(RoleManager<CrmIdentityRole> roleManager)
    : Cheetah.Modules.Identity.Application.Commands.UpdateRoleCommandHandler<CrmIdentityRole>(roleManager)
{
}
