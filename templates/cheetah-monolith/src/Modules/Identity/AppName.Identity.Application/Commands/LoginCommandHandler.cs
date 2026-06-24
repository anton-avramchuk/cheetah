using AppName.Identity.Domain;
using Cheetah.Backend.Rsa.Abstractions;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Services;
using Microsoft.AspNetCore.Identity;

namespace AppName.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<LoginCommand, TokenResult>))]
public class LoginCommandHandler(
    UserManager<AppNameIdentityUser> userManager,
    RoleManager<AppNameIdentityRole> roleManager,
    ITokenGenerator tokenGenerator,
    IPasswordDecryptor passwordDecryptor)
    : Cheetah.Modules.Identity.Application.Commands.LoginCommandHandler<AppNameIdentityUser, AppNameIdentityRole>(
        userManager, roleManager, tokenGenerator, passwordDecryptor)
{
}
