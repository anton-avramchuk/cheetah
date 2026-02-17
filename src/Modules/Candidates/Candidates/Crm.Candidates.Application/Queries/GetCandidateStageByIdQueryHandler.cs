using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Candidates.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Candidates.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetCandidateStageByIdQuery, CandidateStageModel?>))]
public class GetCandidateStageByIdQueryHandler : IQueryHandler<GetCandidateStageByIdQuery, CandidateStageModel?>
{
    private readonly IReadOnlyRepository<CandidateStage, Guid> _repository;

    public GetCandidateStageByIdQueryHandler(IRepository<CandidateStage, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<CandidateStageModel?> HandleAsync(GetCandidateStageByIdQuery query, CancellationToken ct = default)
    {
        var entity = await _repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new CandidateStageModel(entity.Id, entity.Name, entity.Order, entity.Color?.Value, entity.IsDefault);
    }
}
