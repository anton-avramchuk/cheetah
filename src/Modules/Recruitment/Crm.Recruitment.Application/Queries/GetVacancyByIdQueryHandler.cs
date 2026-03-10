using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Crm.Recruitment.Domain;
using Cheetah.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetVacancyByIdQuery, VacancyModel?>))]
public class GetVacancyByIdQueryHandler(IRepository<Vacancy, Guid> repository, IObjectMapper mapper)
    : IQueryHandler<GetVacancyByIdQuery, VacancyModel?>
{
    public async ValueTask<VacancyModel?> HandleAsync(GetVacancyByIdQuery query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<VacancyModel>(repository.AsNoTrackingQueryable().Where(new EntityByIdSpecification<Vacancy>(query.Id)))
            .FirstOrDefaultAsync(ct);
    }
}
