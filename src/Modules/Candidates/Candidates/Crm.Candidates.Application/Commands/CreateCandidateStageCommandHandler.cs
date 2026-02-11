using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Candidates.Domain;

namespace Crm.Candidates.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateCandidateStageCommand, Guid>))]
public class CreateCandidateStageCommandHandler : ICommandHandler<CreateCandidateStageCommand, Guid>
{
    private readonly IRepository<CandidateStage, Guid> _repository;

    public CreateCandidateStageCommandHandler(IRepository<CandidateStage, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<Guid> HandleAsync(CreateCandidateStageCommand command, CancellationToken ct = default)
    {
        var entity = CandidateStage.Create(command.Name, command.Order, command.Color, command.IsDefault);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);
        return entity.Id;
    }
}
