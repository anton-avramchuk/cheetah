using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateStackItemCommand>))]
public class UpdateStackItemCommandHandler(IRepository<StackItem, Guid> repository)
    : ICommandHandler<UpdateStackItemCommand>
{
    public async ValueTask HandleAsync(UpdateStackItemCommand command, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<StackItem>(command.Id);

        entity.Update(command.Name, command.Description);
        await repository.SaveChangesAsync(ct);

    }
}