using Cheetah.Core.EntityFramework;
using Crm.VacancyTasks.DataAccess.Configurations;
using Crm.VacancyTasks.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.VacancyTasks.DataAccess;

public class VacancyTasksDbContext(DbContextOptions<VacancyTasksDbContext> options)
    : CrmDbContext<VacancyTasksDbContext>(options)
{
    public DbSet<VacancyTask> VacancyTasks => Set<VacancyTask>();

    public DbSet<TaskState> TaskStates => Set<TaskState>();

    public DbSet<TaskPriority> TaskPriorities => Set<TaskPriority>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new VacancyTaskConfiguration());
        modelBuilder.ApplyConfiguration(new TaskStateConfiguration());
        modelBuilder.ApplyConfiguration(new TaskPriorityConfiguration());
    }
}
