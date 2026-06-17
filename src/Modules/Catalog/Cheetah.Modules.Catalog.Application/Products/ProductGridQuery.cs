using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.Grid;
using Cheetah.Modules.Catalog.Contracts;
using Cheetah.Modules.Catalog.Domain.Entities;

namespace Cheetah.Modules.Catalog.Application.Products;

/// <summary>
/// Грид товаров (пагинация/сортировка/фильтрация). Generic по <typeparamref name="TGridViewModel"/> —
/// расширяемому табличному ViewModel наследника (как <c>GetUsersGridQuery&lt;TUserModel&gt;</c> в Identity).
/// </summary>
public sealed record GetProductsGridQuery<TGridViewModel>(
    int Page, int PageSize, List<SortDescriptor> Sort, FilterDescriptor? Filter)
    : IQuery<GridResult<TGridViewModel>>
    where TGridViewModel : ProductGridViewModelBase;

public class GetProductsGridQueryHandler<TProduct, TGridViewModel>
    : IQueryHandler<GetProductsGridQuery<TGridViewModel>, GridResult<TGridViewModel>>
    where TProduct : ProductBase
    where TGridViewModel : ProductGridViewModelBase
{
    private readonly IGridRepository<TProduct, Guid> _repository;

    public GetProductsGridQueryHandler(IGridRepository<TProduct, Guid> repository)
        => _repository = repository;

    public async ValueTask<GridResult<TGridViewModel>> HandleAsync(
        GetProductsGridQuery<TGridViewModel> query, CancellationToken ct = default)
    {
        var request = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };
        return await _repository.GetGridAsync<TGridViewModel>(request, ct);
    }
}
