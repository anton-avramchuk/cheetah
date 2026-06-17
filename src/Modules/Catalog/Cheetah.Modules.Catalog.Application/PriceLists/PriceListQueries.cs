using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Modules.Catalog.Contracts;
using Cheetah.Modules.Catalog.Domain.Entities;
using Cheetah.Modules.Catalog.Domain.Specifications;

namespace Cheetah.Modules.Catalog.Application.PriceLists;

// ── Грид прайс-листов ────────────────────────────────────────────────────────────────────────

/// <summary>Грид прайс-листов (пагинация/сортировка/фильтрация, без строк).</summary>
public sealed record GetPriceListsGridQuery(int Page, int PageSize, List<SortDescriptor> Sort, FilterDescriptor? Filter)
    : IQuery<GridResult<PriceListGridViewModel>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetPriceListsGridQuery, GridResult<PriceListGridViewModel>>))]
public sealed class GetPriceListsGridQueryHandler
    : IQueryHandler<GetPriceListsGridQuery, GridResult<PriceListGridViewModel>>
{
    private readonly IGridRepository<PriceList, Guid> _repository;

    public GetPriceListsGridQueryHandler(IGridRepository<PriceList, Guid> repository)
        => _repository = repository;

    public async ValueTask<GridResult<PriceListGridViewModel>> HandleAsync(
        GetPriceListsGridQuery query, CancellationToken ct = default)
    {
        var request = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };
        return await _repository.GetGridAsync<PriceListGridViewModel>(request, ct);
    }
}

// ── Прайс-лист по Id ─────────────────────────────────────────────────────────────────────────

/// <summary>Получить прайс-лист со строками по идентификатору (null, если не найден).</summary>
public sealed record GetPriceListByIdQuery(Guid Id) : IQuery<PriceListDto?>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetPriceListByIdQuery, PriceListDto?>))]
public sealed class GetPriceListByIdQueryHandler : IQueryHandler<GetPriceListByIdQuery, PriceListDto?>
{
    private readonly IRepository<PriceList, Guid> _repository;

    public GetPriceListByIdQueryHandler(IRepository<PriceList, Guid> repository)
        => _repository = repository;

    public async ValueTask<PriceListDto?> HandleAsync(GetPriceListByIdQuery query, CancellationToken ct = default)
    {
        var priceList = await _repository.GetBySpecAsync(new PriceListByIdSpecification(query.Id), ct);
        return priceList is null ? null : ToDto(priceList);
    }

    internal static PriceListDto ToDto(PriceList pl) => new()
    {
        Id = pl.Id,
        Name = pl.Name,
        Currency = pl.Currency,
        IsDefault = pl.IsDefault,
        ValidFrom = pl.ValidFrom,
        ValidTo = pl.ValidTo,
        Items = pl.Items
            .Select(i => new PriceListItemDto { Id = i.Id, ProductId = i.ProductId, Price = i.Price, MinQty = i.MinQty })
            .ToArray(),
        CreatedAt = pl.CreatedAt,
        UpdatedAt = pl.UpdatedAt
    };
}

// ── Разрешение цены ──────────────────────────────────────────────────────────────────────────

/// <summary>
/// Разрешить цену товара при заданном количестве. Если прайс-лист не указан — берётся прайс
/// «по умолчанию». Возвращает null, если прайс/цена не найдены.
/// </summary>
public sealed record ResolvePriceQuery(Guid? PriceListId, Guid ProductId, int Qty) : IQuery<ResolvedPriceDto?>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<ResolvePriceQuery, ResolvedPriceDto?>))]
public sealed class ResolvePriceQueryHandler : IQueryHandler<ResolvePriceQuery, ResolvedPriceDto?>
{
    private readonly IRepository<PriceList, Guid> _repository;

    public ResolvePriceQueryHandler(IRepository<PriceList, Guid> repository)
        => _repository = repository;

    public async ValueTask<ResolvedPriceDto?> HandleAsync(ResolvePriceQuery query, CancellationToken ct = default)
    {
        var priceList = query.PriceListId is { } id
            ? await _repository.GetBySpecAsync(new PriceListByIdSpecification(id), ct)
            : await _repository.GetBySpecAsync(new DefaultPriceListSpecification(), ct);

        if (priceList is null)
            return null;

        var price = priceList.ResolvePrice(query.ProductId, query.Qty);
        return price is null
            ? null
            : new ResolvedPriceDto(priceList.Id, query.ProductId, price.Value, priceList.Currency);
    }
}
