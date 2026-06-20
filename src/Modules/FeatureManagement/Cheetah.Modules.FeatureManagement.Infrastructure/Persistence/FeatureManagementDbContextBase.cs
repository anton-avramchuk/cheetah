using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Modules.FeatureManagement.Domain.Entities;
using Cheetah.Modules.FeatureManagement.Infrastructure.Persistence.Configurations;
using Cheetah.Modules.FeatureManagement.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.FeatureManagement.Infrastructure.Persistence;

/// <summary>
/// Абстрактный generic-DbContext шаблонного модуля FeatureManagement. Хостит флаги
/// (<typeparamref name="TFlag"/>) и их детей (правила/варианты/override) в одной БД. Наследник
/// закрывает его конкретным типом флага и поставляет конфигурацию через
/// <see cref="CreateFlagConfiguration"/>. Миграции — у наследника / в <c>.Default</c>.
/// </summary>
[ConnectionStringName(FeatureManagementConstants.ConnectionStringName)]
public abstract class FeatureManagementDbContextBase<TContext, TFlag> : CrmDbContext<TContext>
    where TContext : DbContext
    where TFlag : FeatureFlagBase
{
    public DbSet<TFlag> FeatureFlags => Set<TFlag>();
    public DbSet<TargetingRule> TargetingRules => Set<TargetingRule>();
    public DbSet<FeatureVariantDef> FeatureVariants => Set<FeatureVariantDef>();
    public DbSet<TenantOverride> TenantOverrides => Set<TenantOverride>();

    protected FeatureManagementDbContextBase(DbContextOptions<TContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(CreateFlagConfiguration());
        modelBuilder.ApplyConfiguration(new TargetingRuleConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureVariantDefConfiguration());
        modelBuilder.ApplyConfiguration(new TenantOverrideConfiguration());
    }

    /// <summary>Конкретная конфигурация сущности флага, поставляемая наследником.</summary>
    protected abstract IEntityTypeConfiguration<TFlag> CreateFlagConfiguration();
}
