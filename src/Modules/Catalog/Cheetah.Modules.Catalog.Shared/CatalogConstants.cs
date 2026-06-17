namespace Cheetah.Modules.Catalog.Shared;

/// <summary>
/// Общие константы шаблонного модуля Catalog: имя БД-подключения, ограничения длин, дефолтные
/// имена таблиц/схемы и префиксы маршрутов. Используются абстрактными базами
/// (<c>ProductConfigurationBase</c>, <c>ProductEndpointsBase</c>) как значения по умолчанию, которые
/// наследник может переопределить.
/// </summary>
public static class CatalogConstants
{
    public const string ConnectionStringName = "Catalog";

    public const int MaxSkuLength = 64;
    public const int MaxNameLength = 300;
    public const int MaxDescriptionLength = 4000;
    public const int MaxCurrencyLength = 3;
    public const int MaxCategoryPathLength = 1024;

    public const string DefaultSchema = "catalog";
    public const string DefaultProductsTableName = "Products";
    public const string DefaultCategoriesTableName = "ProductCategories";
    public const string DefaultPriceListsTableName = "PriceLists";
    public const string DefaultPriceListItemsTableName = "PriceListItems";

    public const string DefaultProductsRoutePrefix = "api/products";
    public const string DefaultCategoriesRoutePrefix = "api/categories";
    public const string DefaultPriceListsRoutePrefix = "api/price-lists";
}
