using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Modules.Workflow.Domain.Entities;
using Cheetah.Modules.Workflow.Infrastructure.Persistence.Configurations;
using Cheetah.Modules.Workflow.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Workflow.Infrastructure.Persistence;

/// <summary>
/// Абстрактный generic-DbContext шаблонного модуля Workflow: правила (<typeparamref name="TRule"/>) с
/// триггерами/действиями + журнал срабатываний. Наследник закрывает его конкретным типом правила и
/// поставляет конфигурацию через <see cref="CreateRuleConfiguration"/>. Миграции — у наследника / в <c>.Default</c>.
/// </summary>
[ConnectionStringName(WorkflowConstants.ConnectionStringName)]
public abstract class WorkflowDbContextBase<TContext, TRule> : CrmDbContext<TContext>
    where TContext : DbContext
    where TRule : AutomationRuleBase
{
    public DbSet<TRule> Rules => Set<TRule>();
    public DbSet<TriggerBinding> TriggerBindings => Set<TriggerBinding>();
    public DbSet<RuleAction> RuleActions => Set<RuleAction>();
    public DbSet<AutomationRun> Runs => Set<AutomationRun>();
    public DbSet<AutomationRunStep> RunSteps => Set<AutomationRunStep>();

    protected WorkflowDbContextBase(DbContextOptions<TContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(CreateRuleConfiguration());
        modelBuilder.ApplyConfiguration(new TriggerBindingConfiguration());
        modelBuilder.ApplyConfiguration(new RuleActionConfiguration());
        modelBuilder.ApplyConfiguration(new AutomationRunConfiguration());
        modelBuilder.ApplyConfiguration(new AutomationRunStepConfiguration());
    }

    /// <summary>Конкретная конфигурация сущности правила, поставляемая наследником.</summary>
    protected abstract Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<TRule> CreateRuleConfiguration();
}
