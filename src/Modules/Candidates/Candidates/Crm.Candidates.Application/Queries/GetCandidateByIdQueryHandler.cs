using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Candidates.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Candidates.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetCandidateByIdQuery, CandidateModel?>))]
public class GetCandidateByIdQueryHandler : IQueryHandler<GetCandidateByIdQuery, CandidateModel?>
{
    private readonly IReadOnlyRepository<Candidate, Guid> _repository;

    public GetCandidateByIdQueryHandler(IRepository<Candidate, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<CandidateModel?> HandleAsync(GetCandidateByIdQuery query, CancellationToken ct = default)
    {
        var entity = await _repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new CandidateModel(
            entity.Id,
            entity.FirstName,
            entity.LastName,
            entity.Email,
            entity.Phone,
            entity.City,
            entity.CurrentPosition,
            entity.CurrentCompany,
            entity.SalaryExpectation,
            entity.About);
    }
}