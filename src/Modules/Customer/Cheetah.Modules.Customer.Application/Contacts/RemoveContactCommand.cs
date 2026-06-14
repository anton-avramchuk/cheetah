using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Customer.Application.Exceptions;
using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Application.Contacts;

/// <summary>Удалить контактное лицо (мягкое удаление).</summary>
public sealed record RemoveContactCommand(Guid Id) : ICommand;

public class RemoveContactCommandHandler<TContact> : ICommandHandler<RemoveContactCommand>
    where TContact : ContactBase
{
    private readonly IRepository<TContact, Guid> _repository;
    private readonly IEventBus _eventBus;

    public RemoveContactCommandHandler(IRepository<TContact, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(RemoveContactCommand command, CancellationToken ct = default)
    {
        var contact = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new CustomerValidationException($"Contact '{command.Id}' not found");

        contact.Remove();
        await _repository.SaveChangesAsync(ct);

        foreach (var e in contact.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        contact.ClearDomainEvents();
    }
}
