using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.VacancyTasks.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.VacancyTasks.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<TaskPriority>), typeof(IRepository<TaskPriority, Guid>))]
public class TaskPriorityRepository : EfGridRepository<VacancyTasksDbContext, TaskPriority>
{
    public TaskPriorityRepository(VacancyTasksDbContext context, IObjectMapper mapper, ILogger<TaskPriorityRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
