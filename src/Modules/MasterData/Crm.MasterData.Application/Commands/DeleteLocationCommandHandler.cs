using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteLocationCommand>))]
public class DeleteLocationCommandHandler(IRepository<Location, Guid> repository)
    : ICommandHandler<DeleteLocationCommand>
{
    public async ValueTask HandleAsync(DeleteLocationCommand command, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<Location>(command.Id);
        repository.Delete(entity);
        await repository.SaveChangesAsync(ct);
    }
}
