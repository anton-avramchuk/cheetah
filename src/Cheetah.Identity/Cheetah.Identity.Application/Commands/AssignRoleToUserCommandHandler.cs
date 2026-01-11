using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Identity.Domain.Repositories;
using Cheetah.Identity.Events;

namespace Cheetah.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<AssignRoleToUserCommand>))]
public class AssignRoleToUserCommandHandler : ICommandHandler<AssignRoleToUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IEventBus _eventBus;

    public AssignRoleToUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IEventBus eventBus)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(AssignRoleToUserCommand command, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, ct);
        if (user == null)
            throw new InvalidOperationException($"User {command.UserId} not found");

        var role = await _roleRepository.GetByIdAsync(command.RoleId, ct);
        if (role == null)
            throw new InvalidOperationException($"Role {command.RoleId} not found");

        // Assign role to user
        user.AddRole(role);

        await _userRepository.SaveChangesAsync(ct);

        // Publish event
        await _eventBus.PublishAsync(new UserRoleAssignedEvent(
            user.Id,
            user.Email,
            role.Id,
            role.Name,
            DateTime.UtcNow
        ), ct);
    }
}
