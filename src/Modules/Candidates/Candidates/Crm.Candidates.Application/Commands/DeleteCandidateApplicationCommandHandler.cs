using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Candidates.Domain;

namespace Crm.Candidates.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteCandidateApplicationCommand>))]
public class DeleteCandidateApplicationCommandHandler : ICommandHandler<DeleteCandidateApplicationCommand>
{
    private readonly IRepository<CandidateApplication, Guid> _repository;

    public DeleteCandidateApplicationCommandHandler(IRepository<CandidateApplication, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(DeleteCandidateApplicationCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<CandidateApplication>(command.Id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(ct);
    }
}
