using System.ComponentModel.DataAnnotations;

namespace Crm.Recruitment.Frontend.Models;

public class VacancyStateFormModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(256, ErrorMessage = "Name must not exceed 256 characters")]
    public string Name { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Order must be non-negative")]
    public int Order { get; set; }

    public string? Color { get; set; }

    public bool IsDefault { get; set; }

    public bool IsEdit => Id.HasValue;
}
