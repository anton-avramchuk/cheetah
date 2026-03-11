using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateStackItemCommand>))]
public class UpdateStackItemCommandHandler : ICommandHandler<UpdateStackItemCommand>
{
    private readonly IRepository<StackItem, Guid> _repository;
    private readonly IEventBus _eventBus;

    public UpdateStackItemCommandHandler(IRepository<StackItem, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UpdateStackItemCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<StackItem>(command.Id);

        entity.Update(command.Name, command.Description);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();
    }
}