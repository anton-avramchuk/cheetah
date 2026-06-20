using Cheetah.Modules.FeatureManagement.Domain.Entities;
using Cheetah.Modules.FeatureManagement.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.FeatureManagement.Infrastructure.Persistence.Configurations;

/// <summary>EF-конфигурация правила таргетинга (дитя флага). Конкретный тип — без наследования.</summary>
public sealed class TargetingRuleConfiguration : IEntityTypeConfiguration<TargetingRule>
{
    public void Configure(EntityTypeBuilder<TargetingRule> builder)
    {
        builder.ToTable(FeatureManagementConstants.RulesTableName, FeatureManagementConstants.DefaultSchema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FilterName).HasMaxLength(FeatureManagementConstants.MaxFilterNameLength).IsRequired();
        builder.Property(x => x.ParametersJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.ResultVariant).HasMaxLength(FeatureManagementConstants.MaxVariantNameLength);
        builder.HasIndex(x => new { x.FlagId, x.Order });
    }
}

/// <summary>EF-конфигурация варианта A/B (дитя флага).</summary>
public sealed class FeatureVariantDefConfiguration : IEntityTypeConfiguration<FeatureVariantDef>
{
    public void Configure(EntityTypeBuilder<FeatureVariantDef> builder)
    {
        builder.ToTable(FeatureManagementConstants.VariantsTableName, FeatureManagementConstants.DefaultSchema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(FeatureManagementConstants.MaxVariantNameLength).IsRequired();
        builder.HasIndex(x => x.FlagId);
    }
}

/// <summary>EF-конфигурация override-а тенанта (дитя флага).</summary>
public sealed class TenantOverrideConfiguration : IEntityTypeConfiguration<TenantOverride>
{
    public void Configure(EntityTypeBuilder<TenantOverride> builder)
    {
        builder.ToTable(FeatureManagementConstants.TenantOverridesTableName, FeatureManagementConstants.DefaultSchema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RulesJson).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.FlagId, x.TenantId }).IsUnique();
    }
}
