using Cheetah.Core.EntityFramework;
using Crm.VacancyTasks.DataAccess.Configurations;
using Crm.VacancyTasks.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.VacancyTasks.DataAccess;

public class VacancyTasksDbContext(DbContextOptions<VacancyTasksDbContext> options)
    : CrmDbContext<VacancyTasksDbContext>(options)
{
    public DbSet<VacancyTask> SampleEntities => Set<VacancyTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new VacancyTaskConfiguration());
    }
}