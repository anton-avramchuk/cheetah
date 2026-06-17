using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Modules.Catalog.Contracts;
using Cheetah.Modules.Catalog.Domain.Entities;

namespace Cheetah.Modules.Catalog.Application.Categories;

// ── Категория по Id ──────────────────────────────────────────────────────────────────────────

/// <summary>Получить категорию по идентификатору (null, если не найдена).</summary>
public sealed record GetCategoryByIdQuery(Guid Id) : IQuery<ProductCategoryDto?>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetCategoryByIdQuery, ProductCategoryDto?>))]
public sealed class GetCategoryByIdQueryHandler : IQueryHandler<GetCategoryByIdQuery, ProductCategoryDto?>
{
    private readonly IGridRepository<ProductCategory, Guid> _repository;

    public GetCategoryByIdQueryHandler(IGridRepository<ProductCategory, Guid> repository)
        => _repository = repository;

    public async ValueTask<ProductCategoryDto?> HandleAsync(GetCategoryByIdQuery query, CancellationToken ct = default)
        => await _repository.GetByIdAsync<ProductCategoryDto>(query.Id, ct);
}

// ── Грид категорий ───────────────────────────────────────────────────────────────────────────

/// <summary>Грид категорий (пагинация/сортировка/фильтрация).</summary>
public sealed record GetCategoriesGridQuery(int Page, int PageSize, List<SortDescriptor> Sort, FilterDescriptor? Filter)
    : IQuery<GridResult<ProductCategoryDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetCategoriesGridQuery, GridResult<ProductCategoryDto>>))]
public sealed class GetCategoriesGridQueryHandler
    : IQueryHandler<GetCategoriesGridQuery, GridResult<ProductCategoryDto>>
{
    private readonly IGridRepository<ProductCategory, Guid> _repository;

    public GetCategoriesGridQueryHandler(IGridRepository<ProductCategory, Guid> repository)
        => _repository = repository;

    public async ValueTask<GridResult<ProductCategoryDto>> HandleAsync(
        GetCategoriesGridQuery query, CancellationToken ct = default)
    {
        var request = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };
        return await _repository.GetGridAsync<ProductCategoryDto>(request, ct);
    }
}
