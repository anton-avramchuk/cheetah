using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.AspNetCore.Blazor.Navigation.Builders;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Modules.Catalog.Blazor.Navigation;

/// <summary>
/// Пункты бокового меню модуля Catalog: секция «Каталог» со страницами товаров, категорий и прайс-листов.
/// Регистрируется генератором по <c>[Export]</c>; вручную в DI добавлять не нужно.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IMenuContributor))]
public sealed class CatalogMenuContributor : IMenuContributor
{
    public string TargetMenuId => Constants.MainMenuId;

    public Task ConfigureMenuAsync(MenuBuilder builder)
    {
        var section = builder.AddSection("catalog", "Каталог", order: 30);

        section.AddItem("catalog-products", "Товары")
            .WithIcon("bi bi-box-seam")
            .WithUrl("catalog/products")
            .WithOrder(0);

        section.AddItem("catalog-categories", "Категории")
            .WithIcon("bi bi-diagram-3")
            .WithUrl("catalog/categories")
            .WithOrder(1);

        section.AddItem("catalog-price-lists", "Прайс-листы")
            .WithIcon("bi bi-tags")
            .WithUrl("catalog/price-lists")
            .WithOrder(2);

        return Task.CompletedTask;
    }
}
