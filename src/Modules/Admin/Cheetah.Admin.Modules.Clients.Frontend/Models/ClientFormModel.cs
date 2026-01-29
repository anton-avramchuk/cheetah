using System.ComponentModel.DataAnnotations;

namespace Cheetah.Admin.Modules.Clients.Frontend.Models;

/// <summary>
/// Form model for creating/editing a client
/// </summary>
public class ClientFormModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
    public string Description { get; set; } = string.Empty;

    public bool IsEdit => Id.HasValue;
}
