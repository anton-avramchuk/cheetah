using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.VacancyTasks.Domain;
using Cheetah.Mapping.Core;
using Microsoft.Extensions.Logging;

namespace Crm.VacancyTasks.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<VacancyTask>), typeof(IRepository<VacancyTask, Guid>))]
public class VacancyTaskRepository : EfGridRepository<VacancyTasksDbContext, VacancyTask>
{
    public VacancyTaskRepository(VacancyTasksDbContext context, IObjectMapper mapper,
        ILogger<VacancyTaskRepository> logger)
        : base(context, mapper, logger)
    {
    }
}