using AppName.Identity.Domain;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.Application.Commands;
using Microsoft.AspNetCore.Identity;

namespace AppName.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateUserCommand>))]
public class UpdateUserCommandHandler(UserManager<AppNameIdentityUser> userManager, RoleManager<AppNameIdentityRole> roleManager)
    : Cheetah.Modules.Identity.Application.Commands.UpdateUserCommandHandler<AppNameIdentityUser, AppNameIdentityRole>(userManager, roleManager)
{
}
