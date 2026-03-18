using AppName.Domain;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;

namespace AppName.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateSampleEntityCommand, Guid>))]
public class CreateSampleEntityCommandHandler : ICommandHandler<CreateSampleEntityCommand, Guid>
{
    private readonly IRepository<SampleEntity, Guid> _repository;
    private readonly IEventBus _eventBus;

    public CreateSampleEntityCommandHandler(IRepository<SampleEntity, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateSampleEntityCommand command, CancellationToken ct = default)
    {
        var entity = SampleEntity.Create(command.Name, command.Description);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();

        return entity.Id;
    }
}
