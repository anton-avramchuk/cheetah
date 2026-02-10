using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteCustomerDirectionCommand>))]
public class DeleteCustomerDirectionCommandHandler : ICommandHandler<DeleteCustomerDirectionCommand>
{
    private readonly IRepository<CustomerDirection, Guid> _repository;

    public DeleteCustomerDirectionCommandHandler(IRepository<CustomerDirection, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(DeleteCustomerDirectionCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<CustomerDirection>(command.Id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(ct);
    }
}
