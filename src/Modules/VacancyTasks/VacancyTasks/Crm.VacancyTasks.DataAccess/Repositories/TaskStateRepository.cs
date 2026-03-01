using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.VacancyTasks.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.VacancyTasks.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<TaskState>), typeof(IRepository<TaskState, Guid>))]
public class TaskStateRepository : EfGridRepository<VacancyTasksDbContext, TaskState>
{
    public TaskStateRepository(VacancyTasksDbContext context, IObjectMapper mapper, ILogger<TaskStateRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
