using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateLocationCommand>))]
public class UpdateLocationCommandHandler(IRepository<Location, Guid> repository)
    : ICommandHandler<UpdateLocationCommand>
{
    public async ValueTask HandleAsync(UpdateLocationCommand command, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<Location>(command.Id);
        entity.Update(command.Country, command.City, command.Timezone);
        await repository.SaveChangesAsync(ct);
    }
}
