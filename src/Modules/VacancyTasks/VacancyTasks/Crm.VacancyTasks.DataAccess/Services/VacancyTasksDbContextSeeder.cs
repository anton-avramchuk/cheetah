using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Seeding;
using Crm.VacancyTasks.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.VacancyTasks.DataAccess.Services;

[Export(LifetimeType.Scoped, typeof(IDatabaseSeeder))]
public class VacancyTasksDbContextSeeder(VacancyTasksDbContext context) : IDatabaseSeeder
{
    private readonly VacancyTasksDbContext _context = context;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedTaskStatesAsync(cancellationToken);
        await SeedTaskPrioritiesAsync(cancellationToken);
    }

    private async Task SeedTaskStatesAsync(CancellationToken cancellationToken)
    {
        if (await _context.TaskStates.AnyAsync(cancellationToken))
            return;

        await _context.AddRangeAsync(
            TaskState.Create("To do", 0, "#6c757d", isDefault: true),
            TaskState.Create("In Progress", 1, "#0d6efd"),
            TaskState.Create("In Review", 2, "#fd7e14"),
            TaskState.Create("Done", 3, "#198754"),
            TaskState.Create("Cancelled", 4, "#dc3545")
        );

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedTaskPrioritiesAsync(CancellationToken cancellationToken)
    {
        if (await _context.TaskPriorities.AnyAsync(cancellationToken))
            return;

        await _context.AddRangeAsync(
            TaskPriority.Create("Critical", 0, "#dc3545"),
            TaskPriority.Create("High", 1, "#fd7e14"),
            TaskPriority.Create("Medium", 2, "#ffc107"),
            TaskPriority.Create("Low", 3, "#198754")
        );

        await _context.SaveChangesAsync(cancellationToken);
    }
}
