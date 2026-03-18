using AppName.Identity.Domain;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.Application.Commands;
using Microsoft.AspNetCore.Identity;

namespace AppName.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteRoleCommand>))]
public class DeleteRoleCommandHandler(RoleManager<AppNameIdentityRole> roleManager)
    : Cheetah.Modules.Identity.Application.Commands.DeleteRoleCommandHandler<AppNameIdentityRole>(roleManager)
{
}
