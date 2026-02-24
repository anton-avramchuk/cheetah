using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Crm.Identity.Frontend.Models;

public class UserFormModel : IValidatableObject
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [StringLength(256, ErrorMessage = "Username must not exceed 256 characters")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(256, ErrorMessage = "Email must not exceed 256 characters")]
    public string Email { get; set; } = string.Empty;

    [StringLength(256, ErrorMessage = "Password must not exceed 256 characters")]
    public string? Password { get; set; }

    public bool IsEdit => Id.HasValue;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsEdit && string.IsNullOrWhiteSpace(Password))
            yield return new ValidationResult("Password is required", [nameof(Password)]);
    }
}
