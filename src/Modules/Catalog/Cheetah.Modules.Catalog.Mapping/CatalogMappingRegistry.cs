using Cheetah.Mapping.Core;
using Cheetah.Modules.Catalog.Application.Categories;
using Cheetah.Modules.Catalog.Application.PriceLists;
using Cheetah.Modules.Catalog.Contracts;
using Cheetah.Modules.Catalog.Domain.Entities;

namespace Cheetah.Modules.Catalog.Mapping;

/// <summary>
/// Реестр генерируемых мапперов Catalog (аналог Mapster-профиля, но через source generator).
/// Маркер размещён в выделенной маппинг-сборке, поэтому Contracts/Domain остаются чистыми.
/// Request → Command/Query — без проекции; Entity → Dto/ViewModel — read-проекции (нужен ProjectTo).
/// Маппинги <c>SetPriceRequest</c>/<c>ResolvePriceRequest</c> требуют переименования (PriceListId ← Id)
/// и остаются за Mapster.
/// </summary>
// ── Categories ──────────────────────────────────────────────────────────────
[GenerateMapper(typeof(CreateCategoryRequest), typeof(CreateCategoryCommand), GenerateProjection = false)]
[GenerateMapper(typeof(UpdateCategoryRequest), typeof(UpdateCategoryCommand), GenerateProjection = false)]
[GenerateMapper(typeof(GetCategoryByIdRequest), typeof(GetCategoryByIdQuery), GenerateProjection = false)]
[GenerateMapper(typeof(DeleteCategoryRequest), typeof(DeleteCategoryCommand), GenerateProjection = false)]
[GenerateMapper(typeof(GetCategoriesGridRequest), typeof(GetCategoriesGridQuery), GenerateProjection = false)]
[GenerateMapper(typeof(ProductCategory), typeof(ProductCategoryDto))] // read-проекция: grid + GetById
// ── Price lists ─────────────────────────────────────────────────────────────
[GenerateMapper(typeof(CreatePriceListRequest), typeof(CreatePriceListCommand), GenerateProjection = false)]
[GenerateMapper(typeof(UpdatePriceListRequest), typeof(UpdatePriceListCommand), GenerateProjection = false)]
[GenerateMapper(typeof(GetPriceListByIdRequest), typeof(GetPriceListByIdQuery), GenerateProjection = false)]
[GenerateMapper(typeof(DeletePriceListRequest), typeof(DeletePriceListCommand), GenerateProjection = false)]
[GenerateMapper(typeof(GetPriceListsGridRequest), typeof(GetPriceListsGridQuery), GenerateProjection = false)]
[GenerateMapper(typeof(PriceList), typeof(PriceListGridViewModel))] // read-проекция: grid
public static partial class CatalogMappingRegistry
{
}
