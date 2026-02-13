using System.ComponentModel.DataAnnotations;

namespace Crm.Recruitment.Frontend.Models;

public class CustomerDirectionFormModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(256, ErrorMessage = "Name must not exceed 256 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(4000, ErrorMessage = "Description must not exceed 4000 characters")]
    public string? Description { get; set; }

    public bool IsEdit => Id.HasValue;
}
