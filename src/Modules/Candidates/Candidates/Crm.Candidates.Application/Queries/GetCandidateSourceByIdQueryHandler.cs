using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Candidates.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Candidates.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetCandidateSourceByIdQuery, CandidateSourceModel?>))]
public class GetCandidateSourceByIdQueryHandler : IQueryHandler<GetCandidateSourceByIdQuery, CandidateSourceModel?>
{
    private readonly IReadOnlyRepository<CandidateSource, Guid> _repository;

    public GetCandidateSourceByIdQueryHandler(IRepository<CandidateSource, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<CandidateSourceModel?> HandleAsync(GetCandidateSourceByIdQuery query, CancellationToken ct = default)
    {
        var entity = await _repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new CandidateSourceModel(entity.Id, entity.Name, entity.Order, entity.Color);
    }
}
