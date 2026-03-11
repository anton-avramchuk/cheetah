using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateSkillCategoryCommand>))]
public class UpdateSkillCategoryCommandHandler(IRepository<SkillCategory, Guid> repository)
    : ICommandHandler<UpdateSkillCategoryCommand>
{
    public async ValueTask HandleAsync(UpdateSkillCategoryCommand command, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<SkillCategory>(command.Id);
        entity.Update(command.Name);
        await repository.SaveChangesAsync(ct);
    }
}
