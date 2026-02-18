using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateCustomerCommand>))]
public class UpdateCustomerCommandHandler : ICommandHandler<UpdateCustomerCommand>
{
    private readonly IRepository<Customer, Guid> _repository;

    public UpdateCustomerCommandHandler(IRepository<Customer, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(UpdateCustomerCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<Customer>(command.Id);

        entity.Update(command.Name, command.Description);
        entity.SetCode(command.Code);
        entity.SetDirection(command.DirectionId);
        await _repository.SaveChangesAsync(ct);
    }
}
