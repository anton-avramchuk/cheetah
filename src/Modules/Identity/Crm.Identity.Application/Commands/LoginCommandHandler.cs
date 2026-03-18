using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.Application.Exceptions;
using Cheetah.Modules.Identity.Application.Services;
using Cheetah.Modules.Identity.Domain;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Crm.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<LoginCommand, TokenResult>))]
public class LoginCommandHandler(
    UserManager<CrmIdentityUser> userManager,
    ITokenGenerator tokenGenerator)
    : ICommandHandler<LoginCommand, TokenResult>
{
    public async ValueTask<TokenResult> HandleAsync(LoginCommand command, CancellationToken ct = default)
    {
        var user = await userManager.FindByNameAsync(command.UserName);
        if (user is null)
            throw new InvalidCredentialsException();

        var isPasswordValid = await userManager.CheckPasswordAsync(user, command.Password);
        if (!isPasswordValid)
            throw new InvalidCredentialsException();

        var roles = await userManager.GetRolesAsync(user);

        var userName = user.UserName ?? throw new InvalidOperationException("User account is in an invalid state.");
        var email = user.Email ?? throw new InvalidOperationException("User account is in an invalid state.");

        return tokenGenerator.GenerateToken(user.Id, userName, email, roles);
    }
}
