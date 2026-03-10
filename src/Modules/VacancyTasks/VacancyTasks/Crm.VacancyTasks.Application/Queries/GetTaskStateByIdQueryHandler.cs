using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Crm.VacancyTasks.Domain;
using Cheetah.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.VacancyTasks.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetTaskStateByIdQuery, TaskStateModel?>))]
public class GetTaskStateByIdQueryHandler(IRepository<TaskState, Guid> repository, IObjectMapper mapper)
    : IQueryHandler<GetTaskStateByIdQuery, TaskStateModel?>
{
    public async ValueTask<TaskStateModel?> HandleAsync(GetTaskStateByIdQuery query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<TaskStateModel>(repository.AsNoTrackingQueryable().Where(new EntityByIdSpecification<TaskState>(query.Id)))
            .FirstOrDefaultAsync(ct);
    }
}
