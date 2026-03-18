using AppName.Identity.Domain;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.Application.Commands;
using Microsoft.AspNetCore.Identity;

namespace AppName.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateUserCommand, Guid>))]
public class CreateUserCommandHandler(UserManager<AppNameIdentityUser> userManager, RoleManager<AppNameIdentityRole> roleManager)
    : Cheetah.Modules.Identity.Application.Commands.CreateUserCommandHandler<AppNameIdentityUser, AppNameIdentityRole>(userManager, roleManager)
{
    protected override AppNameIdentityUser CreateUser(CreateUserCommand command)
        => AppNameIdentityUser.Create(command.UserName, command.Email);
}
