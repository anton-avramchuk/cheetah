using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using __Prefix__.ModuleName.Domain;

namespace __Prefix__.ModuleName.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateSampleEntityCommand>))]
public class UpdateSampleEntityCommandHandler : ICommandHandler<UpdateSampleEntityCommand>
{
    private readonly IRepository<SampleEntity, Guid> _repository;
    private readonly IEventBus _eventBus;

    public UpdateSampleEntityCommandHandler(IRepository<SampleEntity, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UpdateSampleEntityCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<SampleEntity>(command.Id);

        entity.Update(command.Name, command.Description);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();
    }
}
