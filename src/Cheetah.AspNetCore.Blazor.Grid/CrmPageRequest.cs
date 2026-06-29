using Cheetah.Contracts.Requests;

namespace Cheetah.AspNetCore.Blazor.Grid;

/// <summary>
/// UI-запрос грида: пагинация + сортировка + фильтрация. Сортировка/фильтрация переиспользуют те же
/// дескрипторы, что и серверный <see cref="GridRequest"/> (<see cref="SortDescriptor"/>/
/// <see cref="FilterDescriptor"/>) — <see cref="BaseCrudService{TEntity,TGrid,TDetails,TCreate}"/>
/// прокидывает их в репозиторий без перекладки.
/// </summary>
public class CrmPageRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    /// <summary>Дескрипторы сортировки (поле + направление asc/desc). Несколько элементов = ThenBy.</summary>
    public List<SortDescriptor> Sort { get; set; } = [];

    /// <summary>Корневой дескриптор фильтрации (рекурсивный AND/OR). <c>null</c> — без фильтра.</summary>
    public FilterDescriptor? Filter { get; set; }
}
