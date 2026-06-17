using Cheetah.Modules.Catalog.Domain.Entities;
using Cheetah.Modules.Catalog.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>Конкретная EF-конфигурация прайс-листа и его строк (агрегат + дочерние позиции).</summary>
public sealed class PriceListConfiguration : IEntityTypeConfiguration<PriceList>
{
    public void Configure(EntityTypeBuilder<PriceList> builder)
    {
        builder.ToTable(CatalogConstants.DefaultPriceListsTableName, CatalogConstants.DefaultSchema);
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(CatalogConstants.MaxNameLength).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(CatalogConstants.MaxCurrencyLength).IsRequired();

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(i => i.PriceListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasIndex(x => x.IsDefault);
    }
}

/// <summary>Конкретная EF-конфигурация строки прайс-листа.</summary>
public sealed class PriceListItemConfiguration : IEntityTypeConfiguration<PriceListItem>
{
    public void Configure(EntityTypeBuilder<PriceListItem> builder)
    {
        builder.ToTable(CatalogConstants.DefaultPriceListItemsTableName, CatalogConstants.DefaultSchema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Price).HasColumnType("numeric(18,4)");

        builder.HasIndex(x => new { x.PriceListId, x.ProductId });
    }
}
