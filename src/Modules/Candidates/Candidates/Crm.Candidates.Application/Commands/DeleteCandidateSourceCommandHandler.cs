using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Candidates.Domain;

namespace Crm.Candidates.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteCandidateSourceCommand>))]
public class DeleteCandidateSourceCommandHandler : ICommandHandler<DeleteCandidateSourceCommand>
{
    private readonly IRepository<CandidateSource, Guid> _repository;

    public DeleteCandidateSourceCommandHandler(IRepository<CandidateSource, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(DeleteCandidateSourceCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<CandidateSource>(command.Id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(ct);
    }
}
