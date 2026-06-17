using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Modules.Catalog.Shared;

namespace Cheetah.Modules.Catalog.Contracts;

// ── ViewModels (конкретные) ──────────────────────────────────────────────────────────────────

/// <summary>«Лёгкий» ViewModel прайс-листа для грида (без строк).</summary>
public sealed record PriceListGridViewModel : ICrmResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Currency { get; init; } = null!;
    public bool IsDefault { get; init; }
    public DateTimeOffset? ValidFrom { get; init; }
    public DateTimeOffset? ValidTo { get; init; }
}

/// <summary>Полный ViewModel прайс-листа со строками (деталь).</summary>
public sealed record PriceListDto : ICrmResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Currency { get; init; } = null!;
    public bool IsDefault { get; init; }
    public DateTimeOffset? ValidFrom { get; init; }
    public DateTimeOffset? ValidTo { get; init; }
    public IReadOnlyList<PriceListItemDto> Items { get; init; } = Array.Empty<PriceListItemDto>();
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

/// <summary>Строка прайс-листа: цена товара (опц. от порога количества).</summary>
public sealed record PriceListItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public decimal Price { get; init; }
    public int? MinQty { get; init; }
}

/// <summary>Результат разрешения цены товара (с учётом количества и прайс-листа).</summary>
public sealed record ResolvedPriceDto(Guid PriceListId, Guid ProductId, decimal Price, string Currency) : ICrmResponse;

// ── Requests ─────────────────────────────────────────────────────────────────────────────────

[ApiRoute(CatalogConstants.DefaultPriceListsRoutePrefix, ApiMethod.Create, ServiceName = "PriceLists")]
public sealed record CreatePriceListRequest(
    string Name, string Currency, bool IsDefault = false,
    DateTimeOffset? ValidFrom = null, DateTimeOffset? ValidTo = null) : ICrmRequest;

[ApiRoute(CatalogConstants.DefaultPriceListsRoutePrefix + "/{id:guid}", ApiMethod.Update, ServiceName = "PriceLists")]
public sealed record UpdatePriceListRequest(
    [FromRoute] Guid Id, string Name, bool IsDefault = false,
    DateTimeOffset? ValidFrom = null, DateTimeOffset? ValidTo = null) : ICrmRequest;

[ApiRoute(CatalogConstants.DefaultPriceListsRoutePrefix + "/{id:guid}", ApiMethod.GetOrNotFound,
    ResponseType = typeof(PriceListDto), ServiceName = "PriceLists")]
public sealed record GetPriceListByIdRequest([FromRoute] Guid Id) : ICrmRequest;

[ApiRoute(CatalogConstants.DefaultPriceListsRoutePrefix + "/{id:guid}", ApiMethod.Delete, ServiceName = "PriceLists")]
public sealed record DeletePriceListRequest([FromRoute] Guid Id) : ICrmRequest;

[ApiRoute(CatalogConstants.DefaultPriceListsRoutePrefix, ApiMethod.GetGrid,
    ResponseType = typeof(PriceListGridViewModel), ServiceName = "PriceLists")]
public sealed class GetPriceListsGridRequest : GridRequest;

[ApiRoute(CatalogConstants.DefaultPriceListsRoutePrefix + "/{id:guid}/items", ApiMethod.Post, ServiceName = "PriceLists")]
public sealed record SetPriceRequest([FromRoute] Guid Id, Guid ProductId, decimal Price, int? MinQty = null) : ICrmRequest;

[ApiRoute(CatalogConstants.DefaultPriceListsRoutePrefix + "/{id:guid}/price", ApiMethod.GetOrNotFound,
    ResponseType = typeof(ResolvedPriceDto), ServiceName = "PriceLists")]
public sealed record ResolvePriceRequest([FromRoute] Guid Id, [FromQuery] Guid ProductId, [FromQuery] int Qty = 1) : ICrmRequest;
