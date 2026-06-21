using Cheetah.Modules.Workflow.Domain.Entities;
using Cheetah.Modules.Workflow.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Workflow.Infrastructure.Persistence.Configurations;

/// <summary>Базовая конфигурация правила. Наследник переопределяет <see cref="ConfigureCustom"/> для доп. полей.</summary>
public abstract class AutomationRuleConfigurationBase<TRule> : IEntityTypeConfiguration<TRule>
    where TRule : AutomationRuleBase
{
    public void Configure(EntityTypeBuilder<TRule> b)
    {
        b.ToTable("AutomationRules", WorkflowConstants.Schema);
        b.HasKey(r => r.Id);
        b.Property(r => r.Name).HasMaxLength(300).IsRequired();
        b.Property(r => r.OwnerService).HasMaxLength(100).IsRequired();
        b.Property(r => r.Description).HasMaxLength(2000);
        b.Property(r => r.ConditionExpression).HasColumnType("jsonb");

        b.HasMany(r => r.Triggers).WithOne().HasForeignKey(t => t.RuleId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(r => r.Actions).WithOne().HasForeignKey(a => a.RuleId).OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(r => r.OwnerService);
        b.HasIndex(r => r.IsActive);

        b.Ignore(r => r.DomainEvents);
        ConfigureCustom(b);
    }

    protected virtual void ConfigureCustom(EntityTypeBuilder<TRule> b) { }
}

public sealed class TriggerBindingConfiguration : IEntityTypeConfiguration<TriggerBinding>
{
    public void Configure(EntityTypeBuilder<TriggerBinding> b)
    {
        b.ToTable("TriggerBindings", WorkflowConstants.Schema);
        b.HasKey(t => t.Id);
        b.Property(t => t.TriggerType).HasConversion<int>();
        b.Property(t => t.TriggerKey).HasMaxLength(200).IsRequired();
        b.Property(t => t.Parameters).HasColumnType("jsonb");
        b.HasIndex(t => new { t.TriggerType, t.TriggerKey });
    }
}

public sealed class RuleActionConfiguration : IEntityTypeConfiguration<RuleAction>
{
    public void Configure(EntityTypeBuilder<RuleAction> b)
    {
        b.ToTable("RuleActions", WorkflowConstants.Schema);
        b.HasKey(a => a.Id);
        b.Property(a => a.ActionType).HasMaxLength(100).IsRequired();
        b.Property(a => a.Parameters).HasColumnType("jsonb").IsRequired();
        b.Property(a => a.FailureMode).HasConversion<int>();
        b.HasIndex(a => new { a.RuleId, a.Order });
    }
}

public sealed class AutomationRunConfiguration : IEntityTypeConfiguration<AutomationRun>
{
    public void Configure(EntityTypeBuilder<AutomationRun> b)
    {
        b.ToTable("AutomationRuns", WorkflowConstants.Schema);
        b.HasKey(r => r.Id);
        b.Property(r => r.EventName).HasMaxLength(200).IsRequired();
        b.Property(r => r.Status).HasConversion<int>();
        b.Property(r => r.PayloadSnapshot).HasColumnType("jsonb");
        b.HasMany(r => r.Steps).WithOne().HasForeignKey(s => s.RunId).OnDelete(DeleteBehavior.Cascade);

        // Дедуп + горячий аудит.
        b.HasIndex(r => new { r.RuleId, r.EventId }).IsUnique();
        b.HasIndex(r => new { r.RuleId, r.CreatedAt });
        b.HasIndex(r => r.Status);

        b.Ignore(r => r.DomainEvents);
    }
}

public sealed class AutomationRunStepConfiguration : IEntityTypeConfiguration<AutomationRunStep>
{
    public void Configure(EntityTypeBuilder<AutomationRunStep> b)
    {
        b.ToTable("AutomationRunSteps", WorkflowConstants.Schema);
        b.HasKey(s => s.Id);
        b.Property(s => s.ActionType).HasMaxLength(100).IsRequired();
        b.Property(s => s.Status).HasConversion<int>();
        b.Property(s => s.Error).HasMaxLength(4000);
    }
}
