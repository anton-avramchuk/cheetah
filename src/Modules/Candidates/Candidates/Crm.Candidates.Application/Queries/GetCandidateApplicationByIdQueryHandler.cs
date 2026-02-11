using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Candidates.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Candidates.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetCandidateApplicationByIdQuery, CandidateApplicationModel?>))]
public class GetCandidateApplicationByIdQueryHandler : IQueryHandler<GetCandidateApplicationByIdQuery, CandidateApplicationModel?>
{
    private readonly IReadOnlyRepository<CandidateApplication, Guid> _repository;

    public GetCandidateApplicationByIdQueryHandler(IRepository<CandidateApplication, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<CandidateApplicationModel?> HandleAsync(GetCandidateApplicationByIdQuery query, CancellationToken ct = default)
    {
        var entity = await _repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new CandidateApplicationModel(
            entity.Id, entity.CandidateId, entity.VacancyId, entity.StageId, entity.Order);
    }
}
