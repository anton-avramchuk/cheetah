using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Customer.Application.Exceptions;
using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Application.Customers;

/// <summary>Обновить базовые поля клиента (имя + контакты).</summary>
public sealed record UpdateCustomerCommand<TUpdateRequest>(Guid Id, TUpdateRequest Request) : ICommand
    where TUpdateRequest : UpdateCustomerRequestBase;

public class UpdateCustomerCommandHandler<TCustomer, TUpdateRequest>
    : ICommandHandler<UpdateCustomerCommand<TUpdateRequest>>
    where TCustomer : CustomerBase
    where TUpdateRequest : UpdateCustomerRequestBase
{
    private readonly IRepository<TCustomer, Guid> _repository;
    private readonly IEventBus _eventBus;

    public UpdateCustomerCommandHandler(IRepository<TCustomer, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UpdateCustomerCommand<TUpdateRequest> command, CancellationToken ct = default)
    {
        var customer = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new CustomerValidationException($"Customer '{command.Id}' not found");

        customer.Rename(command.Request.DisplayName);
        customer.ChangeContacts(command.Request.Email, command.Request.Phone);
        customer.AssignOwner(command.Request.OwnerId);
        await _repository.SaveChangesAsync(ct);

        foreach (var e in customer.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        customer.ClearDomainEvents();
    }
}
