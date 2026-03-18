using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.Application.Commands;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Crm.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateUserCommand, Guid>))]
public class CreateUserCommandHandler(UserManager<CrmIdentityUser> userManager, RoleManager<CrmIdentityRole> roleManager)
    : Cheetah.Modules.Identity.Application.Commands.CreateUserCommandHandler<CrmIdentityUser, CrmIdentityRole>(userManager, roleManager)
{
    protected override CrmIdentityUser CreateUser(CreateUserCommand command) => CrmIdentityUser.Create(command.UserName, command.Email);
}
