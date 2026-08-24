namespace Cheetah.Contracts.Requests;

/// <summary>
/// Запрос для грида с параметрами пагинации, сортировки и фильтрации
/// </summary>
public class GridRequest : ICrmRequest
{
    /// <summary>
    /// Номер страницы (начинается с 1)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>Размер страницы по умолчанию, если клиент его не задал.</summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// Максимально допустимый размер страницы. Значения больше отсекаются на стороне
    /// репозитория: клиент не должен уметь выгрузить таблицу целиком одним запросом.
    /// </summary>
    public const int MaxPageSize = 500;

    /// <summary>
    /// Размер страницы. Значения вне диапазона <c>[1, <see cref="MaxPageSize"/>]</c>
    /// нормализуются: неположительные — к <see cref="DefaultPageSize"/>, слишком большие — к максимуму.
    /// </summary>
    public int PageSize { get; set; } = DefaultPageSize;

    /// <summary>
    /// Список дескрипторов сортировки
    /// </summary>
    public List<SortDescriptor> Sort { get; set; } = [];

    /// <summary>
    /// Корневой дескриптор фильтрации
    /// </summary>
    public FilterDescriptor? Filter { get; set; }
}
