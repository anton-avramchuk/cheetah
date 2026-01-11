using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Identity.Domain.Repositories;

namespace Cheetah.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<AddPermissionToUserCommand>))]
public class AddPermissionToUserCommandHandler : ICommandHandler<AddPermissionToUserCommand>
{
    private readonly IUserRepository _userRepository;

    public AddPermissionToUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async ValueTask HandleAsync(AddPermissionToUserCommand command, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, ct);
        if (user == null)
            throw new InvalidOperationException($"User {command.UserId} not found");

        // Add personal permission to user
        user.AddPermission(command.Permission);

        await _userRepository.SaveChangesAsync(ct);
    }
}
