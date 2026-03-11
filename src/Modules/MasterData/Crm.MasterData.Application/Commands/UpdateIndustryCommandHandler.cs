using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateIndustryCommand>))]
public class UpdateIndustryCommandHandler(IRepository<Industry, Guid> repository)
    : ICommandHandler<UpdateIndustryCommand>
{
    public async ValueTask HandleAsync(UpdateIndustryCommand command, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<Industry>(command.Id);
        entity.ChangeName(command.Name);
        await repository.SaveChangesAsync(ct);
    }
}
