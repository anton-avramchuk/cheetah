using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cheetah.Modules.Workflow.Default.Persistence;

/// <summary>Design-time фабрика для <c>dotnet ef migrations</c>.</summary>
public class WorkflowDbContextFactory : IDesignTimeDbContextFactory<WorkflowDbContext>
{
    public WorkflowDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WorkflowDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=workflow;Username=postgres;Password=postgres");
        return new WorkflowDbContext(optionsBuilder.Options);
    }
}
