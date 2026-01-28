using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;

namespace Cheetah.Admin.Modules.Clients.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateClientCommand, Guid>))]
public class CreateClientCommandHandler : ICommandHandler<CreateClientCommand, Guid>
{
    private readonly IClientRepository _repository;
    private readonly IEventBus _eventBus;

    public CreateClientCommandHandler(IClientRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateClientCommand command, CancellationToken ct = default)
    {
        var entity = Client.Create(command.Name, command.Description);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();

        return entity.Id;
    }
}
