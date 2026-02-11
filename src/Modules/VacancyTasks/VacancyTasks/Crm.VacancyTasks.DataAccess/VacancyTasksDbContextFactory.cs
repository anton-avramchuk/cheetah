using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crm.VacancyTasks.DataAccess;

public class VacancyTasksDbContextFactory : IDesignTimeDbContextFactory<VacancyTasksDbContext>
{
    public VacancyTasksDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<VacancyTasksDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=vacancytasks;Username=postgres;Password=postgres");

        return new VacancyTasksDbContext(optionsBuilder.Options);
    }
}