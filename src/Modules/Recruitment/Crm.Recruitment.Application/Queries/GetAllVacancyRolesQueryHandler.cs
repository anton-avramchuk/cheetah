using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllVacancyRolesQuery, IReadOnlyList<VacancyRoleModel>>))]
public class GetAllVacancyRolesQueryHandler : IQueryHandler<GetAllVacancyRolesQuery, IReadOnlyList<VacancyRoleModel>>
{
    private readonly IReadOnlyRepository<VacancyRole, Guid> _repository;

    public GetAllVacancyRolesQueryHandler(IRepository<VacancyRole, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<IReadOnlyList<VacancyRoleModel>> HandleAsync(
        GetAllVacancyRolesQuery query,
        CancellationToken ct = default)
    {
        var entities = await _repository.AsNoTrackingQueryable()
            .OrderBy(r => r.Order)
            .ToListAsync(ct);

        return entities
            .Select(e => new VacancyRoleModel(e.Id, e.Name, e.Code, e.IsSingle, e.Order))
            .ToList();
    }
}
