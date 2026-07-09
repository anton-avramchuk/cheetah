using Cheetah.Modules.FeatureManagement.Domain.Entities;
using Cheetah.Modules.FeatureManagement.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.FeatureManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Абстрактная базовая EF-конфигурация флага: ключ, игнор доменных событий, колонки, связи с детьми
/// (правила/варианты/override) и индексы. Наследник добавляет свои поля/индексы через
/// <see cref="ConfigureCustom"/> — точка расширения схемы.
/// </summary>
public abstract class FeatureFlagConfigurationBase<TFlag> : IEntityTypeConfiguration<TFlag>
    where TFlag : FeatureFlagBase
{
    protected virtual string TableName => FeatureManagementConstants.FlagsTableName;
    protected virtual string Schema => FeatureManagementConstants.DefaultSchema;

    public virtual void Configure(EntityTypeBuilder<TFlag> builder)
    {
        builder.ToTable(TableName, Schema);
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key).HasMaxLength(FeatureManagementConstants.MaxKeyLength).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(FeatureManagementConstants.MaxNameLength).IsRequired();
        builder.Property(x => x.OwnerService).HasMaxLength(FeatureManagementConstants.MaxOwnerServiceLength).IsRequired();
        builder.Property(x => x.ValueType).HasConversion<int>();
        builder.Property(x => x.ParentKey).HasMaxLength(FeatureManagementConstants.MaxKeyLength);

        builder.HasIndex(x => x.Key).IsUnique();          // ключ — стабильный контракт
        builder.HasIndex(x => x.OwnerService);
        builder.HasIndex(x => x.ParentKey);               // обход потомков конкретного родителя

        // Backing-поля (_rules/_variants/_overrides) находит конвенция; явный HasField на приватных
        // полях базового класса EF не разрешает.
        builder.HasMany(x => x.Rules).WithOne().HasForeignKey(r => r.FlagId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Rules).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Variants).WithOne().HasForeignKey(v => v.FlagId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Variants).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Overrides).WithOne().HasForeignKey(o => o.FlagId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Overrides).UsePropertyAccessMode(PropertyAccessMode.Field);

        ConfigureCustom(builder);
    }

    /// <summary>Hook для доп. полей/индексов наследника.</summary>
    protected virtual void ConfigureCustom(EntityTypeBuilder<TFlag> builder)
    {
    }
}
