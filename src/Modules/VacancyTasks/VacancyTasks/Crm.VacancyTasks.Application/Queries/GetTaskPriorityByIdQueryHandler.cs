using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Crm.VacancyTasks.Domain;
using Crm.VacancyTasks.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Crm.VacancyTasks.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetTaskPriorityByIdQuery, TaskPriorityModel?>))]
public class GetTaskPriorityByIdQueryHandler(IRepository<TaskPriority, Guid> repository, IObjectMapper mapper)
    : IQueryHandler<GetTaskPriorityByIdQuery, TaskPriorityModel?>
{
    public async ValueTask<TaskPriorityModel?> HandleAsync(GetTaskPriorityByIdQuery query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<TaskPriorityModel>(repository.AsNoTrackingQueryable().Where(new EntityByIdSpecification<TaskPriority>(query.Id)))
            .FirstOrDefaultAsync(ct);
    }
}
