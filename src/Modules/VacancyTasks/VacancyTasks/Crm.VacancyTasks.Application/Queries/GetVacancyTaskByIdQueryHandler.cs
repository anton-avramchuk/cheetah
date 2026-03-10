using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Crm.VacancyTasks.Domain;
using Cheetah.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.VacancyTasks.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetVacancyTaskByIdQuery, VacancyTaskModel?>))]
public class GetVacancyTaskByIdQueryHandler(IRepository<VacancyTask, Guid> repository, IObjectMapper mapper)
    : IQueryHandler<GetVacancyTaskByIdQuery, VacancyTaskModel?>
{
    public async ValueTask<VacancyTaskModel?> HandleAsync(GetVacancyTaskByIdQuery query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<VacancyTaskModel>(repository.AsNoTrackingQueryable().Where(new EntityByIdSpecification<VacancyTask>(query.Id)))
            .FirstOrDefaultAsync(ct);
    }
}
