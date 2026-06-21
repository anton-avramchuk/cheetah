using Cheetah.Contracts.Requests;
using Cheetah.Core.Domain;
using Cheetah.Core.Grid;

namespace Cheetah.AspNetCore.Blazor.Grid;

/// <summary>
/// Хелперы скоупинга гридов по владельцу. Если у пользователя нет права видеть все записи —
/// добавляет в <see cref="GridRequest.Filter"/> условие «owner-поле == текущий пользователь»
/// (несколько полей объединяются по OR). Фильтр транслируется репозиторием прямо на сущность.
/// </summary>
public static class DataScopeExtensions
{
    public static async Task<CrmGridResult<TViewModel>> GetScopedGridAsync<TEntity, TViewModel>(
        this IGridRepository<TEntity> repository,
        CrmPageRequest request,
        DataScope scope,
        CancellationToken ct,
        params string[] ownerFields)
        where TEntity : Entity<Guid>
        where TViewModel : class
    {
        var gridRequest = new GridRequest { Page = request.Page, PageSize = request.PageSize };

        if (!scope.CanViewAll && ownerFields.Length > 0)
            gridRequest.Filter = OwnerFilter(scope.UserId, ownerFields);

        var result = await repository.GetGridAsync<TViewModel>(gridRequest, ct);
        return new CrmGridResult<TViewModel> { Data = result.Data, Total = result.Total };
    }

    private static FilterDescriptor OwnerFilter(Guid userId, string[] ownerFields)
    {
        if (ownerFields.Length == 1)
            return Leaf(ownerFields[0], userId);

        return new FilterDescriptor
        {
            Logic = "or",
            Filters = ownerFields.Select(f => Leaf(f, userId)).ToList()
        };
    }

    private static FilterDescriptor Leaf(string field, Guid userId)
        => new() { Field = field, Operator = "eq", Value = userId };
}
