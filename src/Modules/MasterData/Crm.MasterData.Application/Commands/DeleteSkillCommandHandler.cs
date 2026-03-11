using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteSkillCommand>))]
public class DeleteSkillCommandHandler(IRepository<Skill, Guid> repository)
    : ICommandHandler<DeleteSkillCommand>
{
    public async ValueTask HandleAsync(DeleteSkillCommand command, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<Skill>(command.Id);
        repository.Delete(entity);
        await repository.SaveChangesAsync(ct);
    }
}
