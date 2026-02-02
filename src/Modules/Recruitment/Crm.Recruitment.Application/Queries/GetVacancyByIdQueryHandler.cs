using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain.Repositories;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetVacancyByIdQuery, VacancyModel?>))]
public class GetVacancyByIdQueryHandler : IQueryHandler<GetVacancyByIdQuery, VacancyModel?>
{
    private readonly IVacancyRepository _repository;

    public GetVacancyByIdQueryHandler(IVacancyRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<VacancyModel?> HandleAsync(GetVacancyByIdQuery query, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdNoTrackingAsync(query.Id, ct);
        if (entity is null)
            return null;

        return new VacancyModel(entity.Id, entity.Name, entity.Description);
    }
}