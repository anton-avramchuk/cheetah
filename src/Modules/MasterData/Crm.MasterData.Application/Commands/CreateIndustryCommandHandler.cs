using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateIndustryCommand, Guid>))]
public class CreateIndustryCommandHandler(IRepository<Industry, Guid> repository)
    : ICommandHandler<CreateIndustryCommand, Guid>
{
    public async ValueTask<Guid> HandleAsync(CreateIndustryCommand command, CancellationToken ct = default)
    {
        var entity = Industry.Create(command.Name);
        repository.Add(entity);
        await repository.SaveChangesAsync(ct);
        return entity.Id;
    }
}
