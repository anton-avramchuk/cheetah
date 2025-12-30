using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Identity.Client;
using Cheetah.Identity.Shared.Requests;

namespace Cheetah.Identity.Frontend.Client.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<ChangePasswordCommand>))]
public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly IIdentityClient _identityClient;

    public ChangePasswordCommandHandler(IIdentityClient identityClient)
    {
        _identityClient = identityClient;
    }

    public async ValueTask HandleAsync(ChangePasswordCommand command, CancellationToken ct)
    {
        var request = new ChangePasswordRequest
        {
            CurrentPassword = command.CurrentPassword,
            NewPassword = command.NewPassword
        };

        await _identityClient.ChangePasswordAsync(command.UserId, request, ct);
    }
}
