using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using __Prefix__.ModuleName.Domain;

namespace __Prefix__.ModuleName.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteSampleEntityCommand>))]
public class DeleteSampleEntityCommandHandler : ICommandHandler<DeleteSampleEntityCommand>
{
    private readonly IRepository<SampleEntity, Guid> _repository;
    private readonly IEventBus _eventBus;

    public DeleteSampleEntityCommandHandler(IRepository<SampleEntity, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(DeleteSampleEntityCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new EntityNotFoundException(typeof(SampleEntity), command.Id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();
    }
}
