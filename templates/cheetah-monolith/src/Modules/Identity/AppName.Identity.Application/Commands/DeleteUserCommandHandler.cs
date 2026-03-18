using AppName.Identity.Domain;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.Application.Commands;
using Microsoft.AspNetCore.Identity;

namespace AppName.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteUserCommand>))]
public class DeleteUserCommandHandler(UserManager<AppNameIdentityUser> userManager)
    : Cheetah.Modules.Identity.Application.Commands.DeleteUserCommandHandler<AppNameIdentityUser, AppNameIdentityRole>(userManager)
{
}
