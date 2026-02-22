using Cheetah.Backend.Jwt.Abstractions;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Crm.Identity.Application.Exceptions;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Crm.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<LoginCommand, TokenResult>))]
public class LoginCommandHandler(
    UserManager<CrmUser> userManager,
    IJwtTokenGenerator tokenGenerator)
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
        var result = tokenGenerator.GenerateToken(user.Id, user.UserName!, user.Email!, roles);

        return new TokenResult(result.Token, result.ExpiresInSeconds);
    }
}
