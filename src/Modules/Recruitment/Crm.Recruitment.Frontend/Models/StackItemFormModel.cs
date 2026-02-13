using System.ComponentModel.DataAnnotations;

namespace Crm.Recruitment.Frontend.Models;

public class StackItemFormModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(256, ErrorMessage = "Name must not exceed 256 characters")]
    public string Name { get; set; } = string.Empty;

    public bool IsEdit => Id.HasValue;
}
