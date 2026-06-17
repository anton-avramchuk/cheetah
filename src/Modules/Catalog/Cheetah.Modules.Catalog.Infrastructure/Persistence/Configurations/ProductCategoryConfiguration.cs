using Cheetah.Modules.Catalog.Domain.Entities;
using Cheetah.Modules.Catalog.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>Конкретная EF-конфигурация дерева категорий (self-reference + индекс по пути).</summary>
public sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable(CatalogConstants.DefaultCategoriesTableName, CatalogConstants.DefaultSchema);
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(CatalogConstants.MaxNameLength).IsRequired();
        builder.Property(x => x.Path).HasMaxLength(CatalogConstants.MaxCategoryPathLength).IsRequired();

        builder.HasIndex(x => x.ParentId);
        builder.HasIndex(x => x.Path);
    }
}
