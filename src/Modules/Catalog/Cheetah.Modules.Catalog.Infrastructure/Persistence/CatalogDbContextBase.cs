using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Modules.Catalog.Domain.Entities;
using Cheetah.Modules.Catalog.Infrastructure.Persistence.Configurations;
using Cheetah.Modules.Catalog.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Catalog.Infrastructure.Persistence;

/// <summary>
/// Абстрактный generic-DbContext шаблонного модуля Catalog. Хостит товары
/// (<typeparamref name="TProduct"/>), категории и прайс-листы в одной БД. Наследник закрывает его
/// конкретным типом товара:
/// <c>class AppCatalogDbContext : CatalogDbContextBase&lt;AppCatalogDbContext, Product&gt;</c>
/// и поставляет конфигурацию товара через <see cref="CreateProductConfiguration"/>. Миграции — у
/// наследника. Имя подключения по умолчанию — <see cref="CatalogConstants.ConnectionStringName"/>.
/// </summary>
[ConnectionStringName(CatalogConstants.ConnectionStringName)]
public abstract class CatalogDbContextBase<TContext, TProduct> : CrmDbContext<TContext>
    where TContext : DbContext
    where TProduct : ProductBase
{
    public DbSet<TProduct> Products => Set<TProduct>();
    public DbSet<ProductCategory> Categories => Set<ProductCategory>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceListItem> PriceListItems => Set<PriceListItem>();

    protected CatalogDbContextBase(DbContextOptions<TContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(CreateProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new PriceListConfiguration());
        modelBuilder.ApplyConfiguration(new PriceListItemConfiguration());
    }

    /// <summary>Конкретная конфигурация сущности товара, поставляемая наследником.</summary>
    protected abstract IEntityTypeConfiguration<TProduct> CreateProductConfiguration();
}
