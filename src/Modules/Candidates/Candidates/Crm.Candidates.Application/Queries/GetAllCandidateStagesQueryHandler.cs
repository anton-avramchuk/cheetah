using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Candidates.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Candidates.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllCandidateStagesQuery, IReadOnlyList<CandidateStageModel>>))]
public class GetAllCandidateStagesQueryHandler : IQueryHandler<GetAllCandidateStagesQuery, IReadOnlyList<CandidateStageModel>>
{
    private readonly IReadOnlyRepository<CandidateStage, Guid> _repository;

    public GetAllCandidateStagesQueryHandler(IRepository<CandidateStage, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<IReadOnlyList<CandidateStageModel>> HandleAsync(
        GetAllCandidateStagesQuery query,
        CancellationToken ct = default)
    {
        var entities = await _repository.AsNoTrackingQueryable()
            .OrderBy(e => e.Order)
            .ToListAsync(ct);

        return entities
            .Select(e => new CandidateStageModel(e.Id, e.Name, e.Order, e.Color, e.IsDefault))
            .ToList();
    }
}
