using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Candidates.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Candidates.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllCandidateSourcesQuery, IReadOnlyList<CandidateSourceModel>>))]
public class GetAllCandidateSourcesQueryHandler : IQueryHandler<GetAllCandidateSourcesQuery, IReadOnlyList<CandidateSourceModel>>
{
    private readonly IReadOnlyRepository<CandidateSource, Guid> _repository;

    public GetAllCandidateSourcesQueryHandler(IRepository<CandidateSource, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<IReadOnlyList<CandidateSourceModel>> HandleAsync(
        GetAllCandidateSourcesQuery query,
        CancellationToken ct = default)
    {
        var entities = await _repository.AsNoTrackingQueryable()
            .OrderBy(e => e.Order)
            .ToListAsync(ct);

        return entities
            .Select(e => new CandidateSourceModel(e.Id, e.Name, e.Order, e.Color))
            .ToList();
    }
}
