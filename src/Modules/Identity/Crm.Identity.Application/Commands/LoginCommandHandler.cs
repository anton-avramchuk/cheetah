using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Services;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Crm.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<LoginCommand, TokenResult>))]
public class LoginCommandHandler(UserManager<CrmIdentityUser> userManager, ITokenGenerator tokenGenerator)
    : Cheetah.Modules.Identity.Application.Commands.LoginCommandHandler<CrmIdentityUser, CrmIdentityRole>(userManager, tokenGenerator)
{
}
