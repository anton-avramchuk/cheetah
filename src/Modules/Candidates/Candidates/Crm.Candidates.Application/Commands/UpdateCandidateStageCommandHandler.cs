using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Candidates.Domain;

namespace Crm.Candidates.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateCandidateStageCommand>))]
public class UpdateCandidateStageCommandHandler : ICommandHandler<UpdateCandidateStageCommand>
{
    private readonly IRepository<CandidateStage, Guid> _repository;

    public UpdateCandidateStageCommandHandler(IRepository<CandidateStage, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(UpdateCandidateStageCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<CandidateStage>(command.Id);

        entity.Update(command.Name, command.Order, command.Color, command.IsDefault);
        await _repository.SaveChangesAsync(ct);
    }
}
