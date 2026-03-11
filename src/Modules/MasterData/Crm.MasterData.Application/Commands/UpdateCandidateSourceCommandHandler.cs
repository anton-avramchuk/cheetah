using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateCandidateSourceCommand>))]
public class UpdateCandidateSourceCommandHandler(IRepository<CandidateSource, Guid> repository)
    : ICommandHandler<UpdateCandidateSourceCommand>
{
    public async ValueTask HandleAsync(UpdateCandidateSourceCommand command, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<CandidateSource>(command.Id);
        entity.Update(command.Name);
        await repository.SaveChangesAsync(ct);
    }
}
