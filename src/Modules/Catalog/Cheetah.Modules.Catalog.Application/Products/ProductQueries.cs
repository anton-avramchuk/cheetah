using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Catalog.Application.Abstractions;
using Cheetah.Modules.Catalog.Contracts;
using Cheetah.Modules.Catalog.Domain.Entities;
using Cheetah.Modules.Catalog.Domain.Specifications;

namespace Cheetah.Modules.Catalog.Application.Products;

// ── Товар по Id ──────────────────────────────────────────────────────────────────────────────

/// <summary>Получить товар по идентификатору (null, если не найден).</summary>
public sealed record GetProductByIdQuery<TDto>(Guid Id) : IQuery<TDto?>
    where TDto : ProductDtoBase;

public class GetProductByIdQueryHandler<TProduct, TDto> : IQueryHandler<GetProductByIdQuery<TDto>, TDto?>
    where TProduct : ProductBase
    where TDto : ProductDtoBase
{
    private readonly IRepository<TProduct, Guid> _repository;
    private readonly IProductProjector<TProduct, TDto> _projector;

    public GetProductByIdQueryHandler(IRepository<TProduct, Guid> repository, IProductProjector<TProduct, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<TDto?> HandleAsync(GetProductByIdQuery<TDto> query, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(query.Id, ct);
        return product is null ? null : _projector.ToDto(product);
    }
}

// ── Список товаров ───────────────────────────────────────────────────────────────────────────

/// <summary>Список товаров с комбинированным фильтром (поиск/категория/только активные).</summary>
public sealed record ListProductsQuery<TDto>(string? Search, Guid? CategoryId, bool OnlyActive)
    : IQuery<IReadOnlyList<TDto>>
    where TDto : ProductDtoBase;

public class ListProductsQueryHandler<TProduct, TDto> : IQueryHandler<ListProductsQuery<TDto>, IReadOnlyList<TDto>>
    where TProduct : ProductBase
    where TDto : ProductDtoBase
{
    private readonly IRepository<TProduct, Guid> _repository;
    private readonly IProductProjector<TProduct, TDto> _projector;

    public ListProductsQueryHandler(IRepository<TProduct, Guid> repository, IProductProjector<TProduct, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<IReadOnlyList<TDto>> HandleAsync(ListProductsQuery<TDto> query, CancellationToken ct = default)
    {
        var spec = new ProductsFilterSpecification<TProduct>(query.Search, query.CategoryId, query.OnlyActive);
        var items = await _repository.GetAllAsync(spec, ct);
        return items.Select(_projector.ToDto).ToArray();
    }
}
