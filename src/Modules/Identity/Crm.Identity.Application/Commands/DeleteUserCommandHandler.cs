using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.Application.Commands;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Crm.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteUserCommand>))]
public class DeleteUserCommandHandler(UserManager<CrmIdentityUser> userManager)
    : Cheetah.Modules.Identity.Application.Commands.DeleteUserCommandHandler<CrmIdentityUser, CrmIdentityRole>(userManager)
{
}
