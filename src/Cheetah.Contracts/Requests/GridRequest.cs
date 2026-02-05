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

    /// <summary>
    /// Размер страницы (0 = без пагинации)
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Список дескрипторов сортировки
    /// </summary>
    public List<SortDescriptor> Sort { get; set; } = [];

    /// <summary>
    /// Корневой дескриптор фильтрации
    /// </summary>
    public FilterDescriptor? Filter { get; set; }
}
