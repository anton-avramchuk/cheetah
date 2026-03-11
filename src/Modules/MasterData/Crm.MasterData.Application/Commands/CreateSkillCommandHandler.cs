using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateSkillCommand, Guid>))]
public class CreateSkillCommandHandler(IRepository<Skill, Guid> repository)
    : ICommandHandler<CreateSkillCommand, Guid>
{
    public async ValueTask<Guid> HandleAsync(CreateSkillCommand command, CancellationToken ct = default)
    {
        var entity = Skill.Create(command.Name, command.SkillCategoryId);
        repository.Add(entity);
        await repository.SaveChangesAsync(ct);
        return entity.Id;
    }
}
