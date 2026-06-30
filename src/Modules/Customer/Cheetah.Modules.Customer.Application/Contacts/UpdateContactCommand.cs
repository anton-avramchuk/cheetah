using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Customer.Application.Exceptions;
using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Application.Contacts;

/// <summary>Обновить контактное лицо (имя, должность, контакты).</summary>
public sealed record UpdateContactCommand<TUpdateRequest>(Guid Id, TUpdateRequest Request) : ICommand
    where TUpdateRequest : UpdateContactRequestBase;

public class UpdateContactCommandHandler<TContact, TUpdateRequest, TPosition>
    : ICommandHandler<UpdateContactCommand<TUpdateRequest>>
    where TContact : ContactBase<TPosition>
    where TUpdateRequest : UpdateContactRequestBase
    where TPosition : PositionBase
{
    private readonly IRepository<TContact, Guid> _repository;
    private readonly IEventBus _eventBus;

    public UpdateContactCommandHandler(IRepository<TContact, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UpdateContactCommand<TUpdateRequest> command, CancellationToken ct = default)
    {
        var contact = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new CustomerValidationException($"Contact '{command.Id}' not found");

        contact.Rename(command.Request.FullName);
        contact.ChangePosition(command.Request.PositionId);
        contact.ChangeContacts(command.Request.Email, command.Request.Phone);
        await _repository.SaveChangesAsync(ct);

        foreach (var e in contact.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        contact.ClearDomainEvents();
    }
}
