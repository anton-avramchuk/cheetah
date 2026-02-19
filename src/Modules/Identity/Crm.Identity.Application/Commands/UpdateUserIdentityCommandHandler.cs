using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Crm.Identity.Domain;

namespace Crm.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateUserIdentityCommand>))]
public class UpdateUserIdentityCommandHandler : ICommandHandler<UpdateUserIdentityCommand>
{
    private readonly IRepository<UserIdentity, Guid> _repository;
    private readonly IEventBus _eventBus;

    public UpdateUserIdentityCommandHandler(IRepository<UserIdentity, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UpdateUserIdentityCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<UserIdentity>(command.Id);

        entity.Update(command.Name, command.Description);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();
    }
}