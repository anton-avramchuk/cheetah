using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.Application.Exceptions;
using Cheetah.Modules.Identity.Application.Services;

namespace Cheetah.Modules.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<IssueServiceTokenCommand, TokenResult>))]
public sealed class IssueServiceTokenCommandHandler(
    IServiceClientAuthenticator authenticator,
    ITokenGenerator tokenGenerator)
    : ICommandHandler<IssueServiceTokenCommand, TokenResult>
{
    public ValueTask<TokenResult> HandleAsync(IssueServiceTokenCommand command, CancellationToken ct = default)
    {
        var principal = authenticator.Authenticate(command.ClientId, command.ClientSecret);
        if (principal is null)
            throw new InvalidCredentialsException();

        var token = tokenGenerator.GenerateServiceToken(principal.ClientId, principal.Roles);
        return ValueTask.FromResult(token);
    }
}
