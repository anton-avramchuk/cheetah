using Cheetah.Modules.Customer.Domain.Entities;
using Cheetah.Modules.Customer.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Customer.Infrastructure.Persistence.Configurations;

/// <summary>
/// Абстрактная базовая EF-конфигурация справочной должности: таблица/схема, ключ, аудит-игнор
/// доменных событий, колонка имени и индекс по имени. Наследник наследует её и добавляет свои
/// поля через <see cref="ConfigureCustom"/>.
/// </summary>
public abstract class PositionConfigurationBase<TPosition> : IEntityTypeConfiguration<TPosition>
    where TPosition : PositionBase
{
    protected virtual string TableName => CustomerConstants.DefaultPositionsTableName;
    protected virtual string Schema => CustomerConstants.DefaultSchema;

    public virtual void Configure(EntityTypeBuilder<TPosition> builder)
    {
        builder.ToTable(TableName, Schema);
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(CustomerConstants.MaxNameLength)
            .IsRequired();

        builder.HasIndex(x => x.Name);

        ConfigureCustom(builder);
    }

    /// <summary>Hook для доп. полей/индексов наследника.</summary>
    protected virtual void ConfigureCustom(EntityTypeBuilder<TPosition> builder)
    {
    }
}
