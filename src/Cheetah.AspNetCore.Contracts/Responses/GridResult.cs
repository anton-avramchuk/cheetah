namespace Cheetah.AspNetCore.Contracts.Responses
{
    /// <summary>
    /// Результат выполнения запроса грида
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    public class GridResult<T> : ICrmResponse
    {
        /// <summary>
        /// Данные текущей страницы
        /// </summary>
        public required IEnumerable<T> Data { get; set; }

        /// <summary>
        /// Общее количество записей (без учета пагинации)
        /// </summary>
        public int Total { get; set; }
    }
}