using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Identity.DataAccess;

namespace Cheetah.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<AddPermissionToRoleCommand>))]
public class AddPermissionToRoleCommandHandler : ICommandHandler<AddPermissionToRoleCommand>
{
    private readonly IIdentityDbContext _dbContext;
    private readonly IEventBus _eventBus;

    public AddPermissionToRoleCommandHandler(IIdentityDbContext dbContext, IEventBus eventBus)
    {
        _dbContext = dbContext;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(AddPermissionToRoleCommand command, CancellationToken ct)
    {
        var role = await _dbContext.Roles.FindAsync([command.RoleId], ct);
        if (role == null)
            throw new InvalidOperationException($"Role {command.RoleId} not found");

        // Add permission to role
        role.AddPermission(command.Permission);

        await _dbContext.SaveChangesAsync(ct);

        // Publish domain events
        foreach (var domainEvent in role.DomainEvents)
        {
            await _eventBus.PublishAsync(domainEvent, ct);
        }
        role.ClearDomainEvents();
    }
}
