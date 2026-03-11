using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeletePositionCommand>))]
public class DeletePositionCommandHandler(IRepository<Position, Guid> repository)
    : ICommandHandler<DeletePositionCommand>
{
    public async ValueTask HandleAsync(DeletePositionCommand command, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<Position>(command.Id);
        repository.Delete(entity);
        await repository.SaveChangesAsync(ct);
    }
}
