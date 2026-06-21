using Cheetah.Modules.Workflow.Default.Entities;
using Cheetah.Modules.Workflow.Default.Persistence.Configurations;
using Cheetah.Modules.Workflow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Workflow.Default.Persistence;

/// <summary>Конкретный DbContext «из коробки» для правил <see cref="AutomationRule"/>.</summary>
public sealed class WorkflowDbContext : WorkflowDbContextBase<WorkflowDbContext, AutomationRule>
{
    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options)
    {
    }

    protected override IEntityTypeConfiguration<AutomationRule> CreateRuleConfiguration()
        => new AutomationRuleConfiguration();
}
