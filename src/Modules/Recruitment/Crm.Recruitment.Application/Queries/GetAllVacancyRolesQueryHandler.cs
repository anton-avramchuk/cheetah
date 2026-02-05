using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain.Repositories;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllVacancyRolesQuery, IReadOnlyList<VacancyRoleModel>>))]
public class GetAllVacancyRolesQueryHandler : IQueryHandler<GetAllVacancyRolesQuery, IReadOnlyList<VacancyRoleModel>>
{
    private readonly IVacancyRoleRepository _repository;

    public GetAllVacancyRolesQueryHandler(IVacancyRoleRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<IReadOnlyList<VacancyRoleModel>> HandleAsync(
        GetAllVacancyRolesQuery query,
        CancellationToken ct = default)
    {
        var entities = await _repository.GetAllNoTrackingAsync(ct: ct);

        return entities
            .Select(e => new VacancyRoleModel(e.Id, e.Name, e.Code, e.IsSingle, e.Order))
            .ToList();
    }
}
