using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Customer.Application.Exceptions;
using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Application.Customers;

/// <summary>Архивировать клиента (мягкое удаление).</summary>
public sealed record ArchiveCustomerCommand(Guid Id) : ICommand;

public class ArchiveCustomerCommandHandler<TCustomer> : ICommandHandler<ArchiveCustomerCommand>
    where TCustomer : CustomerBase
{
    private readonly IRepository<TCustomer, Guid> _repository;
    private readonly IEventBus _eventBus;

    public ArchiveCustomerCommandHandler(IRepository<TCustomer, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(ArchiveCustomerCommand command, CancellationToken ct = default)
    {
        var customer = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new CustomerValidationException($"Customer '{command.Id}' not found");

        customer.Archive();
        await _repository.SaveChangesAsync(ct);

        foreach (var e in customer.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        customer.ClearDomainEvents();
    }
}
