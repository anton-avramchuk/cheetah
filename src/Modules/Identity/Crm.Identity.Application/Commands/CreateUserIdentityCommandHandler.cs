using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Crm.Identity.Domain;

namespace Crm.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateUserIdentityCommand, Guid>))]
public class CreateUserIdentityCommandHandler : ICommandHandler<CreateUserIdentityCommand, Guid>
{
    private readonly IRepository<UserIdentity, Guid> _repository;
    private readonly IEventBus _eventBus;

    public CreateUserIdentityCommandHandler(IRepository<UserIdentity, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateUserIdentityCommand command, CancellationToken ct = default)
    {
        var entity = UserIdentity.Create(command.Name, command.Description);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();

        return entity.Id;
    }
}