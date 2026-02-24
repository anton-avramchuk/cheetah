using System.ComponentModel.DataAnnotations;

namespace Crm.Identity.Frontend.Models;

public class LoginFormModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(256, ErrorMessage = "Username must not exceed 256 characters")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(256, ErrorMessage = "Password must not exceed 256 characters")]
    public string Password { get; set; } = string.Empty;
}
