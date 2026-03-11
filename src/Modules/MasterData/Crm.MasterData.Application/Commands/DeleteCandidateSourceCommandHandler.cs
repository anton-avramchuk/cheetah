using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteCandidateSourceCommand>))]
public class DeleteCandidateSourceCommandHandler(IRepository<CandidateSource, Guid> repository)
    : ICommandHandler<DeleteCandidateSourceCommand>
{
    public async ValueTask HandleAsync(DeleteCandidateSourceCommand command, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<CandidateSource>(command.Id);
        repository.Delete(entity);
        await repository.SaveChangesAsync(ct);
    }
}
