using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateStackItemCommand, Guid>))]
public class CreateStackItemCommandHandler : ICommandHandler<CreateStackItemCommand, Guid>
{
    private readonly IRepository<StackItem, Guid> _repository;
    private readonly IEventBus _eventBus;

    public CreateStackItemCommandHandler(IRepository<StackItem, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateStackItemCommand command, CancellationToken ct = default)
    {
        var entity = StackItem.Create(command.Name, command.Description);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();

        return entity.Id;
    }
}