using System.ComponentModel.DataAnnotations;

namespace Crm.VacancyTasks.Frontend.Models;

public class TaskPriorityFormModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(256, ErrorMessage = "Name must not exceed 256 characters")]
    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }

    [StringLength(50, ErrorMessage = "Color must not exceed 50 characters")]
    public string? Color { get; set; }

    public bool IsEdit => Id.HasValue;
}
