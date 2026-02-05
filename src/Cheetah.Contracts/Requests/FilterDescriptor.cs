namespace Cheetah.Contracts.Requests
{
    /// <summary>
    /// Дескриптор фильтрации (рекурсивная структура для поддержки AND/OR)
    /// </summary>
    public class FilterDescriptor
    {
        /// <summary>
        /// Логический оператор для комбинации фильтров: "and" или "or"
        /// </summary>
        public string? Logic { get; set; }

        /// <summary>
        /// Вложенные фильтры (для композитных условий)
        /// </summary>
        public List<FilterDescriptor> Filters { get; set; } = [];

        // --- Свойства для конечного фильтра (leaf node) ---

        /// <summary>
        /// Имя поля для фильтрации (например "Name" или "Patient.Age")
        /// </summary>
        public string? Field { get; set; }

        /// <summary>
        /// Оператор сравнения:
        /// - "eq" (equals)
        /// - "neq" (not equals)
        /// - "contains" (содержит подстроку)
        /// - "startswith" (начинается с)
        /// - "endswith" (заканчивается на)
        /// - "gt" (greater than)
        /// - "gte" (greater than or equal)
        /// - "lt" (less than)
        /// - "lte" (less than or equal)
        /// - "isnull" (is null)
        /// - "isnotnull" (is not null)
        /// - "isempty" (empty string)
        /// - "isnotempty" (not empty string)
        /// </summary>
        public string? Operator { get; set; }

        /// <summary>
        /// Значение для сравнения
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Игнорировать регистр при сравнении строк (по умолчанию true)
        /// </summary>
        public bool IgnoreCase { get; set; } = true;
    }
}
