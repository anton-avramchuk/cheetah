using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Crm.Customer.Domain;

namespace Crm.Customer.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateCustomerCommand, Guid>))]
public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, Guid>
{
    private readonly IRepository<Customer, Guid> _repository;
    private readonly IEventBus _eventBus;

    public CreateCustomerCommandHandler(IRepository<Customer, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateCustomerCommand command, CancellationToken ct = default)
    {
        var entity = Customer.Create(command.Name, command.Description);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();

        return entity.Id;
    }
}