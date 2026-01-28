using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;

namespace Cheetah.Admin.Modules.Clients.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateClientCommand>))]
public class UpdateClientCommandHandler : ICommandHandler<UpdateClientCommand>
{
    private readonly IClientRepository _repository;
    private readonly IEventBus _eventBus;

    public UpdateClientCommandHandler(IClientRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UpdateClientCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw EntityNotFoundException.For<Client>(command.Id);

        entity.Update(command.Name, command.Description);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();
    }
}
