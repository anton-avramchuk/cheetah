using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Identity.DataAccess;
using Cheetah.Identity.Events;

namespace Cheetah.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<AssignRoleToUserCommand>))]
public class AssignRoleToUserCommandHandler : ICommandHandler<AssignRoleToUserCommand>
{
    private readonly IIdentityDbContext _dbContext;
    private readonly IEventBus _eventBus;

    public AssignRoleToUserCommandHandler(IIdentityDbContext dbContext, IEventBus eventBus)
    {
        _dbContext = dbContext;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(AssignRoleToUserCommand command, CancellationToken ct)
    {
        var user = await _dbContext.Users.FindAsync([command.UserId], ct);
        if (user == null)
            throw new InvalidOperationException($"User {command.UserId} not found");

        var role = await _dbContext.Roles.FindAsync([command.RoleId], ct);
        if (role == null)
            throw new InvalidOperationException($"Role {command.RoleId} not found");

        // Assign role to user
        user.AddRole(command.RoleId);

        await _dbContext.SaveChangesAsync(ct);

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
