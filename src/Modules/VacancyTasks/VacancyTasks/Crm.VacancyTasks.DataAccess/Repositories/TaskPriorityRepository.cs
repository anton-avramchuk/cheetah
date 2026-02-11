using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Repositories;
using Crm.VacancyTasks.Domain;

namespace Crm.VacancyTasks.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<TaskPriority, Guid>))]
public class TaskPriorityRepository : EfRepository<VacancyTasksDbContext, TaskPriority>
{
    public TaskPriorityRepository(VacancyTasksDbContext context) : base(context)
    {
    }
}
