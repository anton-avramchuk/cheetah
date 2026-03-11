using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateWorkFormatCommand>))]
public class UpdateWorkFormatCommandHandler(IRepository<WorkFormat, Guid> repository)
    : ICommandHandler<UpdateWorkFormatCommand>
{
    public async ValueTask HandleAsync(UpdateWorkFormatCommand command, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<WorkFormat>(command.Id);
        entity.Update(command.Name);
        await repository.SaveChangesAsync(ct);
    }
}
