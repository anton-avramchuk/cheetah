using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using CustomerEntity = global::Crm.Customer.Domain.Customer;

namespace Crm.Customer.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteCustomerCommand>))]
public class DeleteCustomerCommandHandler : ICommandHandler<DeleteCustomerCommand>
{
    private readonly IRepository<CustomerEntity, Guid> _repository;

    public DeleteCustomerCommandHandler(IRepository<CustomerEntity, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(DeleteCustomerCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<CustomerEntity>(command.Id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(ct);
    }
}
