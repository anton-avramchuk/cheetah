using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateSkillCategoryCommand, Guid>))]
public class CreateSkillCategoryCommandHandler(IRepository<SkillCategory, Guid> repository)
    : ICommandHandler<CreateSkillCategoryCommand, Guid>
{
    public async ValueTask<Guid> HandleAsync(CreateSkillCategoryCommand command, CancellationToken ct = default)
    {
        var entity = SkillCategory.Create(command.Name);
        repository.Add(entity);
        await repository.SaveChangesAsync(ct);
        return entity.Id;
    }
}
