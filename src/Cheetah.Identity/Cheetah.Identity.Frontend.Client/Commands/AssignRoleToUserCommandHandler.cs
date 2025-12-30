using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Identity.Client;

namespace Cheetah.Identity.Frontend.Client.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<AssignRoleToUserCommand>))]
public class AssignRoleToUserCommandHandler : ICommandHandler<AssignRoleToUserCommand>
{
    private readonly IIdentityClient _identityClient;

    public AssignRoleToUserCommandHandler(IIdentityClient identityClient)
    {
        _identityClient = identityClient;
    }

    public async ValueTask HandleAsync(AssignRoleToUserCommand command, CancellationToken ct)
    {
        await _identityClient.AssignRoleToUserAsync(command.UserId, command.RoleId, ct);
    }
}
