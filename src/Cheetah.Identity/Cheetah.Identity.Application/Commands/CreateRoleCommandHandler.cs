using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Identity.DataAccess;
using Cheetah.Identity.Domain.Entities;

namespace Cheetah.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateRoleCommand, Guid>))]
public class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, Guid>
{
    private readonly IIdentityDbContext _dbContext;
    private readonly IEventBus _eventBus;

    public CreateRoleCommandHandler(IIdentityDbContext dbContext, IEventBus eventBus)
    {
        _dbContext = dbContext;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateRoleCommand command, CancellationToken ct)
    {
        // Check if role with this name already exists
        var existingRole = await _dbContext.Roles
            .FirstOrDefaultAsync(r => r.NormalizedName == command.Name.ToUpperInvariant(), ct);

        if (existingRole != null)
            throw new InvalidOperationException($"Role with name '{command.Name}' already exists");

        // Create role
        var role = Role.Create(command.Name, command.Description);

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync(ct);

        // Publish domain events
        foreach (var domainEvent in role.DomainEvents)
        {
            await _eventBus.PublishAsync(domainEvent, ct);
        }
        role.ClearDomainEvents();

        return role.Id;
    }
}
