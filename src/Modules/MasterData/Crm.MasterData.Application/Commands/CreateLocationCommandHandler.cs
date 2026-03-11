using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateLocationCommand, Guid>))]
public class CreateLocationCommandHandler(IRepository<Location, Guid> repository)
    : ICommandHandler<CreateLocationCommand, Guid>
{
    public async ValueTask<Guid> HandleAsync(CreateLocationCommand command, CancellationToken ct = default)
    {
        var entity = Location.Create(command.Country, command.City, command.Timezone);
        repository.Add(entity);
        await repository.SaveChangesAsync(ct);
        return entity.Id;
    }
}
