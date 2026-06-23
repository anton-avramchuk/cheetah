using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.Grid;
using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Domain.Entities;

namespace Cheetah.Modules.Teams.Application.Teams;

/// <summary>
/// Грид команд (пагинация/сортировка/фильтрация). Generic по <typeparamref name="TGridViewModel"/> —
/// расширяемому табличному ViewModel наследника (как <c>GetProductsGridQuery&lt;TGridVm&gt;</c> в Catalog).
/// </summary>
public sealed record GetTeamsGridQuery<TGridViewModel>(
    int Page, int PageSize, List<SortDescriptor> Sort, FilterDescriptor? Filter)
    : IQuery<GridResult<TGridViewModel>>
    where TGridViewModel : TeamGridViewModelBase;

public class GetTeamsGridQueryHandler<TTeam, TGridViewModel>
    : IQueryHandler<GetTeamsGridQuery<TGridViewModel>, GridResult<TGridViewModel>>
    where TTeam : TeamBase
    where TGridViewModel : TeamGridViewModelBase
{
    private readonly IGridRepository<TTeam, Guid> _repository;

    public GetTeamsGridQueryHandler(IGridRepository<TTeam, Guid> repository)
        => _repository = repository;

    public async ValueTask<GridResult<TGridViewModel>> HandleAsync(
        GetTeamsGridQuery<TGridViewModel> query, CancellationToken ct = default)
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
