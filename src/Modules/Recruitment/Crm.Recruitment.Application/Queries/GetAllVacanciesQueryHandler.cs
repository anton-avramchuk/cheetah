using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain.Repositories;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllVacanciesQuery, IReadOnlyList<VacancyModel>>))]
public class GetAllVacanciesQueryHandler : IQueryHandler<GetAllVacanciesQuery, IReadOnlyList<VacancyModel>>
{
    private readonly IVacancyRepository _repository;

    public GetAllVacanciesQueryHandler(IVacancyRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<IReadOnlyList<VacancyModel>> HandleAsync(GetAllVacanciesQuery query,
        CancellationToken ct = default)
    {
        var entities = await _repository.GetAllNoTrackingAsync(ct: ct);

        return entities
            .Select(e => new VacancyModel(e.Id, e.Name, e.Description))
            .ToList();
    }
}
