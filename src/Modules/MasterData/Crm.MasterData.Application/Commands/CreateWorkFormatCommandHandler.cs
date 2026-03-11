using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateWorkFormatCommand, Guid>))]
public class CreateWorkFormatCommandHandler(IRepository<WorkFormat, Guid> repository)
    : ICommandHandler<CreateWorkFormatCommand, Guid>
{
    public async ValueTask<Guid> HandleAsync(CreateWorkFormatCommand command, CancellationToken ct = default)
    {
        var entity = WorkFormat.Create(command.Name);
        repository.Add(entity);
        await repository.SaveChangesAsync(ct);
        return entity.Id;
    }
}
