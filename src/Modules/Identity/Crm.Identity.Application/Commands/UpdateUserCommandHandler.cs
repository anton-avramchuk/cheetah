using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.Application.Commands;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Crm.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateUserCommand>))]
public class UpdateUserCommandHandler(UserManager<CrmIdentityUser> userManager, RoleManager<CrmIdentityRole> roleManager)
    : Cheetah.Modules.Identity.Application.Commands.UpdateUserCommandHandler<CrmIdentityUser, CrmIdentityRole>(userManager, roleManager)
{
}
