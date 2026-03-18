using Cheetah.Core.CQRS;
using Cheetah.Modules.Identity.Application.Exceptions;
using Cheetah.Modules.Identity.Application.Services;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Cheetah.Modules.Identity.Application.Commands;

public abstract class LoginCommandHandler<TUser, TRole>(
    UserManager<TUser> userManager,
    ITokenGenerator tokenGenerator)
    : ICommandHandler<LoginCommand, TokenResult>
    where TRole : IdentityRole
    where TUser : IdentityUser<TRole>
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
