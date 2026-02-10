using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateCustomerDirectionCommand>))]
public class UpdateCustomerDirectionCommandHandler : ICommandHandler<UpdateCustomerDirectionCommand>
{
    private readonly IRepository<CustomerDirection, Guid> _repository;

    public UpdateCustomerDirectionCommandHandler(IRepository<CustomerDirection, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(UpdateCustomerDirectionCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<CustomerDirection>(command.Id);

        entity.Update(command.Name, command.Description);
        await _repository.SaveChangesAsync(ct);
    }
}
