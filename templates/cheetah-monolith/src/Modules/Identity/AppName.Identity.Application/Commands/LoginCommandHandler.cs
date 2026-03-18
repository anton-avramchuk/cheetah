using AppName.Identity.Domain;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Services;
using Microsoft.AspNetCore.Identity;

namespace AppName.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<LoginCommand, TokenResult>))]
public class LoginCommandHandler(UserManager<AppNameIdentityUser> userManager, ITokenGenerator tokenGenerator)
    : Cheetah.Modules.Identity.Application.Commands.LoginCommandHandler<AppNameIdentityUser, AppNameIdentityRole>(userManager, tokenGenerator)
{
}
