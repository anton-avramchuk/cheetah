using AppName.Identity.Domain;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.Application.Commands;
using Microsoft.AspNetCore.Identity;

namespace AppName.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateRoleCommand, Guid>))]
public class CreateRoleCommandHandler(RoleManager<AppNameIdentityRole> roleManager)
    : Cheetah.Modules.Identity.Application.Commands.CreateRoleCommandHandler<AppNameIdentityRole>(roleManager)
{
    protected override AppNameIdentityRole CreateRole(CreateRoleCommand command)
        => AppNameIdentityRole.Create(command.Name);
}
