using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Customer.Application.Abstractions;
using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Application.Customers;

/// <summary>Создать клиента из запроса наследника.</summary>
public sealed record CreateCustomerCommand<TCreateRequest>(TCreateRequest Request) : ICommand<Guid>
    where TCreateRequest : CreateCustomerRequestBase;

public class CreateCustomerCommandHandler<TCustomer, TCreateRequest>
    : ICommandHandler<CreateCustomerCommand<TCreateRequest>, Guid>
    where TCustomer : CustomerBase
    where TCreateRequest : CreateCustomerRequestBase
{
    private readonly ICustomerFactory<TCustomer, TCreateRequest> _factory;
    private readonly IRepository<TCustomer, Guid> _repository;
    private readonly IEventBus _eventBus;

    public CreateCustomerCommandHandler(
        ICustomerFactory<TCustomer, TCreateRequest> factory,
        IRepository<TCustomer, Guid> repository,
        IEventBus eventBus)
    {
        _factory = factory;
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateCustomerCommand<TCreateRequest> command, CancellationToken ct = default)
    {
        var customer = _factory.Create(command.Request);
        _repository.Add(customer);
        await _repository.SaveChangesAsync(ct);

        foreach (var e in customer.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        customer.ClearDomainEvents();

        return customer.Id;
    }
}
