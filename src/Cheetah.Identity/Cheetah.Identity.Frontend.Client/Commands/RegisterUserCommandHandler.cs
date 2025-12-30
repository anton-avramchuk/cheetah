using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Identity.Client;
using Cheetah.Identity.Shared.Requests;
using Cheetah.Identity.Shared.ViewModels;

namespace Cheetah.Identity.Frontend.Client.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<RegisterUserCommand, UserViewModel?>))]
public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, UserViewModel?>
{
    private readonly IIdentityClient _identityClient;

    public RegisterUserCommandHandler(IIdentityClient identityClient)
    {
        _identityClient = identityClient;
    }

    public async ValueTask<UserViewModel?> HandleAsync(RegisterUserCommand command, CancellationToken ct)
    {
        var request = new RegisterUserRequest
        {
            Email = command.Email,
            Password = command.Password,
            FirstName = command.FirstName,
            LastName = command.LastName
        };

        return await _identityClient.RegisterAsync(request, ct);
    }
}
