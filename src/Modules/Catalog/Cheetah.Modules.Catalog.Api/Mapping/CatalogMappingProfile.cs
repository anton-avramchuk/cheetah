using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Catalog.Application.Categories;
using Cheetah.Modules.Catalog.Application.PriceLists;
using Cheetah.Modules.Catalog.Contracts;
using Cheetah.Modules.Catalog.Domain.Entities;
using Mapster;

namespace Cheetah.Modules.Catalog.Api.Mapping;

/// <summary>
/// Mapster-маппинги каталога: Request → Command/Query и Entity → ViewModel (для ProjectTo в гриде и
/// GetById). Покрывает конкретные сущности — категории и прайс-листы. Маппинги расширяемого товара
/// (Request → CreateProductCommand&lt;…&gt;, Product → ViewModel) объявляет наследник/хост.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public sealed class CatalogMappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        // ── Categories ───────────────────────────────────────────────────────────────────────
        config.NewConfig<CreateCategoryRequest, CreateCategoryCommand>();
        config.NewConfig<UpdateCategoryRequest, UpdateCategoryCommand>();
        config.NewConfig<GetCategoryByIdRequest, GetCategoryByIdQuery>();
        config.NewConfig<DeleteCategoryRequest, DeleteCategoryCommand>();
        config.NewConfig<GetCategoriesGridRequest, GetCategoriesGridQuery>();
        config.NewConfig<ProductCategory, ProductCategoryDto>();   // ProjectTo: grid + GetById

        // ── Price lists ──────────────────────────────────────────────────────────────────────
        config.NewConfig<CreatePriceListRequest, CreatePriceListCommand>();
        config.NewConfig<UpdatePriceListRequest, UpdatePriceListCommand>();
        config.NewConfig<GetPriceListByIdRequest, GetPriceListByIdQuery>();
        config.NewConfig<DeletePriceListRequest, DeletePriceListCommand>();
        config.NewConfig<GetPriceListsGridRequest, GetPriceListsGridQuery>();
        config.NewConfig<SetPriceRequest, SetPriceCommand>()
            .Map(dest => dest.PriceListId, src => src.Id);
        config.NewConfig<ResolvePriceRequest, ResolvePriceQuery>()
            .Map(dest => dest.PriceListId, src => src.Id);
        config.NewConfig<PriceList, PriceListGridViewModel>();     // ProjectTo: grid
    }
}
