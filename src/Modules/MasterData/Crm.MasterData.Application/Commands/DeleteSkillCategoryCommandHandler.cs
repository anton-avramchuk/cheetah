using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteSkillCategoryCommand>))]
public class DeleteSkillCategoryCommandHandler(IRepository<SkillCategory, Guid> repository)
    : ICommandHandler<DeleteSkillCategoryCommand>
{
    public async ValueTask HandleAsync(DeleteSkillCategoryCommand command, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<SkillCategory>(command.Id);
        repository.Delete(entity);
        await repository.SaveChangesAsync(ct);
    }
}
