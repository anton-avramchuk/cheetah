using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateStackItemCommand, Guid>))]
public class CreateStackItemCommandHandler(IRepository<StackItem, Guid> repository)
    : ICommandHandler<CreateStackItemCommand, Guid>
{
    public async ValueTask<Guid> HandleAsync(CreateStackItemCommand command, CancellationToken ct = default)
    {
        var entity = StackItem.Create(command.Name, command.Description);
        repository.Add(entity);
        await repository.SaveChangesAsync(ct);

        

        return entity.Id;
    }
}