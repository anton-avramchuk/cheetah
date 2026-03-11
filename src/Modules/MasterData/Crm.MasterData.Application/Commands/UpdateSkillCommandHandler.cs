using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateSkillCommand>))]
public class UpdateSkillCommandHandler(IRepository<Skill, Guid> repository)
    : ICommandHandler<UpdateSkillCommand>
{
    public async ValueTask HandleAsync(UpdateSkillCommand command, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<Skill>(command.Id);
        entity.Update(command.Name, command.SkillCategoryId);
        await repository.SaveChangesAsync(ct);
    }
}
