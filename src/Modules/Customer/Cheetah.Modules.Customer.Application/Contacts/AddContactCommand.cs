using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Customer.Application.Abstractions;
using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Application.Contacts;

/// <summary>Добавить контактное лицо клиенту (CustomerId — из маршрута).</summary>
public sealed record AddContactCommand<TCreateRequest>(Guid CustomerId, TCreateRequest Request) : ICommand<Guid>
    where TCreateRequest : CreateContactRequestBase;

public class AddContactCommandHandler<TContact, TCreateRequest, TPosition>
    : ICommandHandler<AddContactCommand<TCreateRequest>, Guid>
    where TContact : ContactBase<TPosition>
    where TCreateRequest : CreateContactRequestBase
    where TPosition : PositionBase
{
    private readonly IContactFactory<TContact, TCreateRequest, TPosition> _factory;
    private readonly IRepository<TContact, Guid> _repository;
    private readonly IEventBus _eventBus;

    public AddContactCommandHandler(
        IContactFactory<TContact, TCreateRequest, TPosition> factory,
        IRepository<TContact, Guid> repository,
        IEventBus eventBus)
    {
        _factory = factory;
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(AddContactCommand<TCreateRequest> command, CancellationToken ct = default)
    {
        var contact = _factory.Create(command.CustomerId, command.Request);
        _repository.Add(contact);
        await _repository.SaveChangesAsync(ct);

        foreach (var e in contact.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        contact.ClearDomainEvents();

        return contact.Id;
    }
}
