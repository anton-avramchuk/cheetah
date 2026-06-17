using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.Grid;
using Cheetah.Modules.SalesDocuments.Contracts;
using Cheetah.Modules.SalesDocuments.Domain.Entities;

namespace Cheetah.Modules.SalesDocuments.Application.Documents;

/// <summary>
/// Грид документов (пагинация/сортировка/фильтрация). Generic по <typeparamref name="TGridViewModel"/> —
/// расширяемому табличному ViewModel наследника.
/// </summary>
public sealed record GetDocumentsGridQuery<TGridViewModel>(
    int Page, int PageSize, List<SortDescriptor> Sort, FilterDescriptor? Filter)
    : IQuery<GridResult<TGridViewModel>>
    where TGridViewModel : SalesDocumentGridViewModelBase;

public class GetDocumentsGridQueryHandler<TDoc, TGridViewModel>
    : IQueryHandler<GetDocumentsGridQuery<TGridViewModel>, GridResult<TGridViewModel>>
    where TDoc : SalesDocumentBase
    where TGridViewModel : SalesDocumentGridViewModelBase
{
    private readonly IGridRepository<TDoc, Guid> _repository;

    public GetDocumentsGridQueryHandler(IGridRepository<TDoc, Guid> repository)
        => _repository = repository;

    public async ValueTask<GridResult<TGridViewModel>> HandleAsync(
        GetDocumentsGridQuery<TGridViewModel> query, CancellationToken ct = default)
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
