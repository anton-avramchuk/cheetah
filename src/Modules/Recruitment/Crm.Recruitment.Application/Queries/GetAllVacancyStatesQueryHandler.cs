using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllVacancyStatesQuery, IReadOnlyList<VacancyStateModel>>))]
public class GetAllVacancyStatesQueryHandler : IQueryHandler<GetAllVacancyStatesQuery, IReadOnlyList<VacancyStateModel>>
{
    private readonly IReadOnlyRepository<VacancyState, Guid> _repository;

    public GetAllVacancyStatesQueryHandler(IRepository<VacancyState, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<IReadOnlyList<VacancyStateModel>> HandleAsync(
        GetAllVacancyStatesQuery query,
        CancellationToken ct = default)
    {
        var entities = await _repository.AsNoTrackingQueryable()
            .OrderBy(e => e.Order)
            .ToListAsync(ct);

        return entities
            .Select(e => new VacancyStateModel(e.Id, e.Name, e.Order, e.Color?.Value, e.IsDefault))
            .ToList();
    }
}
