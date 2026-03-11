using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateCandidateSourceCommand, Guid>))]
public class CreateCandidateSourceCommandHandler(IRepository<CandidateSource, Guid> repository)
    : ICommandHandler<CreateCandidateSourceCommand, Guid>
{
    public async ValueTask<Guid> HandleAsync(CreateCandidateSourceCommand command, CancellationToken ct = default)
    {
        var entity = CandidateSource.Create(command.Name);
        repository.Add(entity);
        await repository.SaveChangesAsync(ct);
        return entity.Id;
    }
}
