using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Modules.Catalog.Shared;

namespace Cheetah.Modules.Catalog.Contracts;

// ── ViewModels (конкретные — категории не расширяются) ────────────────────────────────────────

/// <summary>ViewModel категории товара (узел дерева). Используется и для грида, и для детали.</summary>
public sealed record ProductCategoryDto : ICrmResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public Guid? ParentId { get; init; }
    public string Path { get; init; } = null!;
    public int Order { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

// ── Requests ─────────────────────────────────────────────────────────────────────────────────

[ApiRoute(CatalogConstants.DefaultCategoriesRoutePrefix, ApiMethod.Create, ServiceName = "Categories")]
public sealed record CreateCategoryRequest(string Name, Guid? ParentId, int Order = 0) : ICrmRequest;

[ApiRoute(CatalogConstants.DefaultCategoriesRoutePrefix + "/{id:guid}", ApiMethod.Update, ServiceName = "Categories")]
public sealed record UpdateCategoryRequest([FromRoute] Guid Id, string Name, int Order = 0) : ICrmRequest;

[ApiRoute(CatalogConstants.DefaultCategoriesRoutePrefix + "/{id:guid}", ApiMethod.GetOrNotFound,
    ResponseType = typeof(ProductCategoryDto), ServiceName = "Categories")]
public sealed record GetCategoryByIdRequest([FromRoute] Guid Id) : ICrmRequest;

[ApiRoute(CatalogConstants.DefaultCategoriesRoutePrefix + "/{id:guid}", ApiMethod.Delete, ServiceName = "Categories")]
public sealed record DeleteCategoryRequest([FromRoute] Guid Id) : ICrmRequest;

[ApiRoute(CatalogConstants.DefaultCategoriesRoutePrefix, ApiMethod.GetGrid,
    ResponseType = typeof(ProductCategoryDto), ServiceName = "Categories")]
public sealed class GetCategoriesGridRequest : GridRequest;
