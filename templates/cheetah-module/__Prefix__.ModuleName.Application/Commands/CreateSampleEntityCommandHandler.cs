using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using __Prefix__.ModuleName.Domain;
using __Prefix__.ModuleName.Domain.Repositories;

namespace __Prefix__.ModuleName.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateSampleEntityCommand, Guid>))]
public class CreateSampleEntityCommandHandler : ICommandHandler<CreateSampleEntityCommand, Guid>
{
    private readonly ISampleEntityRepository _repository;
    private readonly IEventBus _eventBus;

    public CreateSampleEntityCommandHandler(ISampleEntityRepository repository, IEventBus eventBus)
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
