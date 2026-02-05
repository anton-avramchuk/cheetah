namespace Cheetah.Contracts.Responses
{
    /// <summary>
    /// Результат выполнения запроса грида
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    public class GridResult<T> : ICrmResponse
    {
        public GridResult()
        {
            Data = [];
        }

        public GridResult(IEnumerable<T> data, int total)
        {
            Data = data;
            Total = total;
        }

        /// <summary>
        /// Данные текущей страницы
        /// </summary>
        public IEnumerable<T> Data { get; set; }

        /// <summary>
        /// Общее количество записей (без учета пагинации)
        /// </summary>
        public int Total { get; set; }
    }
}
