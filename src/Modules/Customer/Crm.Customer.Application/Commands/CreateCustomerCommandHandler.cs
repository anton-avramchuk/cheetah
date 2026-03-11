using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using CustomerEntity = global::Crm.Customer.Domain.Customer;

namespace Crm.Customer.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateCustomerCommand, Guid>))]
public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, Guid>
{
    private readonly IRepository<CustomerEntity, Guid> _repository;
    private readonly IEventBus _eventBus;

    public CreateCustomerCommandHandler(IRepository<CustomerEntity, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateCustomerCommand command, CancellationToken ct = default)
    {
        var entity = CustomerEntity.Create(command.Name, command.Description);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();

        return entity.Id;
    }
}
