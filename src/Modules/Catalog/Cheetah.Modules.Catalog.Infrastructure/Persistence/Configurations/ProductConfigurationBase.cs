using Cheetah.Modules.Catalog.Domain.Entities;
using Cheetah.Modules.Catalog.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Абстрактная базовая EF-конфигурация товара: ключ, игнор доменных событий, общие колонки и
/// индексы под горячие пути (уникальный Sku, выборка по категории). Наследник наследует её и
/// добавляет свои поля/индексы через <see cref="ConfigureCustom"/> — точка расширения схемы.
/// </summary>
public abstract class ProductConfigurationBase<TProduct> : IEntityTypeConfiguration<TProduct>
    where TProduct : ProductBase
{
    protected virtual string TableName => CatalogConstants.DefaultProductsTableName;
    protected virtual string Schema => CatalogConstants.DefaultSchema;

    public virtual void Configure(EntityTypeBuilder<TProduct> builder)
    {
        builder.ToTable(TableName, Schema);
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Sku).HasMaxLength(CatalogConstants.MaxSkuLength).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(CatalogConstants.MaxNameLength).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(CatalogConstants.MaxDescriptionLength);
        builder.Property(x => x.Type).HasConversion<int>();
        builder.Property(x => x.Unit).HasConversion<int>();
        builder.Property(x => x.Attributes).HasColumnType("jsonb");

        builder.HasIndex(x => x.Sku).IsUnique();
        builder.HasIndex(x => new { x.CategoryId, x.IsActive });

        ConfigureCustom(builder);
    }

    /// <summary>Hook для доп. полей/индексов наследника.</summary>
    protected virtual void ConfigureCustom(EntityTypeBuilder<TProduct> builder)
    {
    }
}
