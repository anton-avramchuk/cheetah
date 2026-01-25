namespace Cheetah.AspNetCore.Contracts.Requests
{
    /// <summary>
    /// Дескриптор сортировки по одному полю
    /// </summary>
    public class SortDescriptor
    {
        /// <summary>
        /// Имя поля для сортировки (например "Name" или "Patient.FirstName")
        /// </summary>
        public string? Field { get; set; }

        /// <summary>
        /// Направление сортировки: "asc" или "desc"
        /// </summary>
        public string? Dir { get; set; }
    }
}
