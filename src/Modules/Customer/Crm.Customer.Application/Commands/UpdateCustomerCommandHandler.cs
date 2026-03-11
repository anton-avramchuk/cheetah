using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using CustomerEntity = global::Crm.Customer.Domain.Customer;

namespace Crm.Customer.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateCustomerCommand>))]
public class UpdateCustomerCommandHandler : ICommandHandler<UpdateCustomerCommand>
{
    private readonly IRepository<CustomerEntity, Guid> _repository;
    private readonly IEventBus _eventBus;

    public UpdateCustomerCommandHandler(IRepository<CustomerEntity, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UpdateCustomerCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<CustomerEntity>(command.Id);

        entity.Update(command.Name, command.Description);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();
    }
}
