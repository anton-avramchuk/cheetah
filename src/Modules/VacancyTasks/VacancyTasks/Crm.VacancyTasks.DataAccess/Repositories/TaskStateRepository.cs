using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Repositories;
using Crm.VacancyTasks.Domain;

namespace Crm.VacancyTasks.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<TaskState, Guid>))]
public class TaskStateRepository : EfRepository<VacancyTasksDbContext, TaskState>
{
    public TaskStateRepository(VacancyTasksDbContext context) : base(context)
    {
    }
}
