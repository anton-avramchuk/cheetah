using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateCustomerCommand, Guid>))]
public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, Guid>
{
    private readonly IRepository<Customer, Guid> _repository;

    public CreateCustomerCommandHandler(IRepository<Customer, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<Guid> HandleAsync(CreateCustomerCommand command, CancellationToken ct = default)
    {
        var entity = Customer.Create(command.Name, command.Description, command.DirectionId, command.Code);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);
        return entity.Id;
    }
}
