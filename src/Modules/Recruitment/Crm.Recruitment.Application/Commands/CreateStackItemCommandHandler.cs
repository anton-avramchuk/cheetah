using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateStackItemCommand, Guid>))]
public class CreateStackItemCommandHandler : ICommandHandler<CreateStackItemCommand, Guid>
{
    private readonly IRepository<StackItem, Guid> _repository;

    public CreateStackItemCommandHandler(IRepository<StackItem, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<Guid> HandleAsync(CreateStackItemCommand command, CancellationToken ct = default)
    {
        var entity = StackItem.Create(command.Name);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);
        return entity.Id;
    }
}
