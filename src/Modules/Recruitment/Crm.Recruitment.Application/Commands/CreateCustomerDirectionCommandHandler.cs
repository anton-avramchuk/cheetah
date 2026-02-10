using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateCustomerDirectionCommand, Guid>))]
public class CreateCustomerDirectionCommandHandler : ICommandHandler<CreateCustomerDirectionCommand, Guid>
{
    private readonly IRepository<CustomerDirection, Guid> _repository;

    public CreateCustomerDirectionCommandHandler(IRepository<CustomerDirection, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<Guid> HandleAsync(CreateCustomerDirectionCommand command, CancellationToken ct = default)
    {
        var entity = CustomerDirection.Create(command.Name, command.Description);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);
        return entity.Id;
    }
}
