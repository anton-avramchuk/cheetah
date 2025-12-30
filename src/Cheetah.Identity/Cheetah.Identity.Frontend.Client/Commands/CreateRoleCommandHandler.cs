using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Identity.Client;
using Cheetah.Identity.Shared.Requests;

namespace Cheetah.Identity.Frontend.Client.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateRoleCommand, Guid>))]
public class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, Guid>
{
    private readonly IIdentityClient _identityClient;

    public CreateRoleCommandHandler(IIdentityClient identityClient)
    {
        _identityClient = identityClient;
    }

    public async ValueTask<Guid> HandleAsync(CreateRoleCommand command, CancellationToken ct)
    {
        var request = new CreateRoleRequest
        {
            Name = command.Name,
            Description = command.Description
        };

        return await _identityClient.CreateRoleAsync(request, ct);
    }
}
