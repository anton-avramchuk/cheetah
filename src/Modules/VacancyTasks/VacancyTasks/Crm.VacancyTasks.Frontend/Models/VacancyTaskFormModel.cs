using System.ComponentModel.DataAnnotations;

namespace Crm.VacancyTasks.Frontend.Models;

/// <summary>
/// Form model for creating/editing a VacancyTask.
/// </summary>
public class VacancyTaskFormModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(256, ErrorMessage = "Title must not exceed 256 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1024, ErrorMessage = "Description must not exceed 1024 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vacancy is required")]
    public Guid VacancyId { get; set; }

    [Required(ErrorMessage = "State is required")]
    public Guid StateId { get; set; }

    public Guid? PriorityId { get; set; }

    public Guid? AssigneeId { get; set; }

    public DateTimeOffset? DueDate { get; set; }

    public bool IsEdit => Id.HasValue;
}
