using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Candidates.Domain;

namespace Crm.Candidates.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteCandidateCommand>))]
public class DeleteCandidateCommandHandler : ICommandHandler<DeleteCandidateCommand>
{
    private readonly IRepository<Candidate, Guid> _repository;

    public DeleteCandidateCommandHandler(IRepository<Candidate, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(DeleteCandidateCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<Candidate>(command.Id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(ct);
    }
}