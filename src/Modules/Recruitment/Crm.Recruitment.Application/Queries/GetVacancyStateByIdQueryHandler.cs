using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Crm.Recruitment.Domain;
using Cheetah.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetVacancyStateByIdQuery, VacancyStateModel?>))]
public class GetVacancyStateByIdQueryHandler(IRepository<VacancyState, Guid> repository, IObjectMapper mapper)
    : IQueryHandler<GetVacancyStateByIdQuery, VacancyStateModel?>
{
    public async ValueTask<VacancyStateModel?> HandleAsync(GetVacancyStateByIdQuery query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<VacancyStateModel>(repository.AsNoTrackingQueryable().Where(new EntityByIdSpecification<VacancyState>(query.Id)))
            .FirstOrDefaultAsync(ct);
    }
}
