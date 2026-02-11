using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Candidates.Domain;

namespace Crm.Candidates.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<MoveCandidateApplicationCommand>))]
public class MoveCandidateApplicationCommandHandler : ICommandHandler<MoveCandidateApplicationCommand>
{
    private readonly IRepository<CandidateApplication, Guid> _repository;

    public MoveCandidateApplicationCommandHandler(IRepository<CandidateApplication, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(MoveCandidateApplicationCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<CandidateApplication>(command.Id);

        entity.Move(command.StageId, command.Order);
        await _repository.SaveChangesAsync(ct);
    }
}
