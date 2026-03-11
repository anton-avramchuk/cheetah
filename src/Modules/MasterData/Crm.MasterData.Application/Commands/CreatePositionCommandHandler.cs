using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreatePositionCommand, Guid>))]
public class CreatePositionCommandHandler(IRepository<Position, Guid> repository)
    : ICommandHandler<CreatePositionCommand, Guid>
{
    public async ValueTask<Guid> HandleAsync(CreatePositionCommand command, CancellationToken ct = default)
    {
        var entity = Position.Create(command.Name, command.Grade);
        repository.Add(entity);
        await repository.SaveChangesAsync(ct);
        return entity.Id;
    }
}
