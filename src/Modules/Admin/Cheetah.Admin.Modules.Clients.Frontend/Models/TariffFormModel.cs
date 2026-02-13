using System.ComponentModel.DataAnnotations;

namespace Cheetah.Admin.Modules.Clients.Frontend.Models;

public class TariffFormModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name must not exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Currency is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be exactly 3 characters")]
    public string Currency { get; set; } = "USD";

    public bool IsActive { get; set; } = true;

    public bool IsEdit => Id.HasValue;
}
