using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdatePositionCommand>))]
public class UpdatePositionCommandHandler(IRepository<Position, Guid> repository)
    : ICommandHandler<UpdatePositionCommand>
{
    public async ValueTask HandleAsync(UpdatePositionCommand command, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<Position>(command.Id);
        entity.Update(command.Name, command.Grade);
        await repository.SaveChangesAsync(ct);
    }
}
