using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Identity.Domain.Entities;
using Cheetah.Identity.Domain.Repositories;

namespace Cheetah.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateRoleCommand, Guid>))]
public class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, Guid>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IEventBus _eventBus;

    public CreateRoleCommandHandler(IRoleRepository roleRepository, IEventBus eventBus)
    {
        _roleRepository = roleRepository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateRoleCommand command, CancellationToken ct)
    {
        // Check if role with this name already exists
        var existingRole = await _roleRepository.GetByNameAsync(command.Name, ct);

        if (existingRole != null)
            throw new InvalidOperationException($"Role with name '{command.Name}' already exists");

        // Create role
        var role = Role.Create(command.Name, command.Description);

        _roleRepository.Add(role);
        await _roleRepository.SaveChangesAsync(ct);

        // Publish domain events
        foreach (var domainEvent in role.DomainEvents)
        {
            await _eventBus.PublishAsync(domainEvent, ct);
        }
        role.ClearDomainEvents();

        return role.Id;
    }
}
